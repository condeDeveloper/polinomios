using System.Numerics;

namespace Conde.Polinomios;

/// <summary>
/// O SORTEIO, escrito aqui em vez de vir da biblioteca.
///
/// A documentacao do sorteio de biblioteca nao promete a mesma sequencia entre
/// versoes nem entre sistemas, e a integracao continua roda em tres. Uma medida
/// de "quantas multiplicacoes o Karatsuba gastou nestes polinomios" com
/// polinomios diferentes em cada sistema nao e uma medida: sao tres medidas que
/// ninguem pode comparar.
/// </summary>
public sealed class Sorteio
{
    private ulong estado;

    public Sorteio(ulong semente) => estado = semente == 0 ? 1 : semente;

    public uint Proximo()
    {
        estado = estado * 6364136223846793005UL + 1442695040888963407UL;
        return (uint)(estado >> 32);
    }

    public int Ate(int limite) => (int)(Proximo() % (uint)limite);

    public int Entre(int menor, int maior) => menor + Ate(maior - menor + 1);

    public double Fracao() => Proximo() / 4294967296.0;
}

/// <summary>
/// Um POLINOMIO de coeficientes inteiros EXATOS.
///
/// Os coeficientes sao inteiros grandes e nao numeros com virgula, e isso nao e
/// capricho: o repositorio inteiro existe para mostrar o que acontece quando eles
/// nao sao. Multiplicar dois polinomios de grau mil com coeficientes de dez
/// digitos da coeficientes de vinte e tres digitos, e o numero com virgula de
/// precisao dupla guarda dezesseis. O resultado nao e "quase certo": e outro
/// polinomio.
///
/// O vetor guarda o coeficiente de cada potencia, do termo constante para cima, e
/// nunca tem zeros sobrando no fim.
/// </summary>
public sealed class Polinomio : IEquatable<Polinomio>
{
    private readonly BigInteger[] coeficientes;

    public Polinomio(IEnumerable<BigInteger> coeficientes)
    {
        ArgumentNullException.ThrowIfNull(coeficientes);

        var lista = coeficientes.ToList();
        while (lista.Count > 0 && lista[^1].IsZero) lista.RemoveAt(lista.Count - 1);

        this.coeficientes = [.. lista];
    }

    public static Polinomio De(params long[] coeficientes) =>
        new(coeficientes.Select(c => (BigInteger)c));

    public static readonly Polinomio Zero = new(Array.Empty<BigInteger>());

    public static readonly Polinomio Um = De(1);

    /// <summary>O grau. O polinomio zero tem grau menos um, por convencao.</summary>
    public int Grau => coeficientes.Length - 1;

    public bool EhZero => coeficientes.Length == 0;

    public int Termos => coeficientes.Length;

    public BigInteger this[int potencia] =>
        potencia >= 0 && potencia < coeficientes.Length ? coeficientes[potencia] : BigInteger.Zero;

    public IReadOnlyList<BigInteger> Coeficientes => coeficientes;

    /// <summary>
    /// O MAIOR COEFICIENTE em valor absoluto, que e a medida de quanto o
    /// polinomio cresceu.
    ///
    /// Ela importa porque a conta exata nao fica errada quando os numeros crescem:
    /// ela fica LENTA. E a conta com numero de precisao fixa nao fica lenta: ela
    /// fica errada. As duas coisas sao medidas aqui lado a lado.
    /// </summary>
    public BigInteger MaiorCoeficiente() =>
        coeficientes.Length == 0 ? BigInteger.Zero : coeficientes.Max(BigInteger.Abs);

    /// <summary>Quantos digitos tem o maior coeficiente.</summary>
    public int DigitosDoMaior()
    {
        var maior = MaiorCoeficiente();
        return maior.IsZero ? 1 : maior.ToString().Length;
    }

    public static Polinomio operator +(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        var tamanho = Math.Max(a.coeficientes.Length, b.coeficientes.Length);
        var soma = new BigInteger[tamanho];

        for (var i = 0; i < tamanho; i++) soma[i] = a[i] + b[i];

        return new Polinomio(soma);
    }

    public static Polinomio operator -(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        var tamanho = Math.Max(a.coeficientes.Length, b.coeficientes.Length);
        var diferenca = new BigInteger[tamanho];

        for (var i = 0; i < tamanho; i++) diferenca[i] = a[i] - b[i];

        return new Polinomio(diferenca);
    }

    public Polinomio Vezes(BigInteger escalar) =>
        new(coeficientes.Select(c => c * escalar));

    /// <summary>Multiplica por x elevado a uma potencia, que e deslocar o vetor.</summary>
    public Polinomio Deslocado(int potencias)
    {
        if (EhZero || potencias == 0) return this;

        var novo = new BigInteger[coeficientes.Length + potencias];
        Array.Copy(coeficientes, 0, novo, potencias, coeficientes.Length);

        return new Polinomio(novo);
    }

    /// <summary>Os termos de grau menor que o corte.</summary>
    public Polinomio ParteBaixa(int corte) =>
        new(coeficientes.Take(Math.Min(corte, coeficientes.Length)));

    /// <summary>Os termos de grau maior ou igual ao corte, abaixados.</summary>
    public Polinomio ParteAlta(int corte) =>
        corte >= coeficientes.Length ? Zero : new Polinomio(coeficientes.Skip(corte));

    /// <summary>
    /// AVALIAR pelo metodo de Horner, que e a unica forma de avaliar que vale a
    /// pena escrever.
    ///
    /// Ele gasta n multiplicacoes e n somas, contra as n(n+1)/2 multiplicacoes de
    /// calcular cada potencia. Com grau mil, sao mil contra meio milhao, e a
    /// resposta e exatamente a mesma.
    /// </summary>
    public BigInteger Em(BigInteger ponto)
    {
        var valor = BigInteger.Zero;

        for (var i = coeficientes.Length - 1; i >= 0; i--) valor = valor * ponto + coeficientes[i];

        return valor;
    }

    /// <summary>Avaliar com as contas feitas modulo um primo.</summary>
    public long Em(long ponto, long primo)
    {
        long valor = 0;

        for (var i = coeficientes.Length - 1; i >= 0; i--)
        {
            var coeficiente = (long)(((coeficientes[i] % primo) + primo) % primo);
            valor = (valor * ponto + coeficiente) % primo;
        }

        return valor;
    }

    /// <summary>
    /// Um polinomio SORTEADO de grau dado, com coeficientes numa faixa.
    ///
    /// O coeficiente do maior grau nunca e zero, porque um polinomio sorteado que
    /// as vezes tem grau menor que o pedido estragaria qualquer medida que fale em
    /// grau.
    /// </summary>
    public static Polinomio Sortear(int grau, Sorteio sorteio, int limite = 1000)
    {
        ArgumentNullException.ThrowIfNull(sorteio);
        if (grau < 0) return Zero;

        var coeficientes = new BigInteger[grau + 1];
        for (var i = 0; i <= grau; i++) coeficientes[i] = sorteio.Entre(-limite, limite);

        while (coeficientes[grau].IsZero) coeficientes[grau] = sorteio.Entre(1, limite);

        return new Polinomio(coeficientes);
    }

    public bool Equals(Polinomio? outro) =>
        outro is not null && coeficientes.AsSpan().SequenceEqual(outro.coeficientes);

    public override bool Equals(object? obj) => Equals(obj as Polinomio);

    public override int GetHashCode()
    {
        var hash = 2166136261u;
        foreach (var c in coeficientes) hash = (hash ^ (uint)c.GetHashCode()) * 16777619u;

        return (int)hash;
    }

    public override string ToString()
    {
        if (EhZero) return "0";

        var partes = new List<string>();

        for (var i = coeficientes.Length - 1; i >= 0; i--)
        {
            if (coeficientes[i].IsZero) continue;

            var termo = i switch
            {
                0 => coeficientes[i].ToString(),
                1 => $"{coeficientes[i]}x",
                _ => $"{coeficientes[i]}x^{i}",
            };

            partes.Add(termo);
        }

        return string.Join(" + ", partes);
    }
}
