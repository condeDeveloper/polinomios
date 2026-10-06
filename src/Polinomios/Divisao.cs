using System.Numerics;

namespace Conde.Polinomios;

/// <summary>
/// A DIVISAO COM RESTO e o maximo divisor comum, e a surpresa que vem junto.
///
/// Dividir polinomio de coeficientes inteiros nem sempre da coeficientes
/// inteiros, e a saida classica e a PSEUDODIVISAO: multiplicar o dividendo por
/// uma potencia do coeficiente lider do divisor antes de dividir, para tudo ficar
/// inteiro.
///
/// Ela funciona, e ela faz os numeros EXPLODIREM. Com dois polinomios de grau
/// dez e coeficientes de tres digitos, o maximo divisor comum pelo algoritmo de
/// Euclides direto passa por coeficientes de centenas de digitos. A resposta
/// final costuma ser minuscula, e o caminho ate ela, nao.
///
/// Esse e um dos exemplos mais claros de uma coisa que a notacao assintotica nao
/// ve: o numero de operacoes e pequeno, e cada operacao fica cara.
/// </summary>
public static class Divisao
{
    public sealed record ComResto(Polinomio Quociente, Polinomio Resto);

    /// <summary>
    /// A PSEUDODIVISAO: multiplica o dividendo pelo lider do divisor elevado ao
    /// que for preciso, e divide.
    ///
    /// Devolve o quociente e o resto, os dois com coeficientes inteiros, e vale a
    /// identidade lider^k vezes dividendo igual a quociente vezes divisor mais
    /// resto. Essa identidade e conferida pelos testes, porque e ela que torna o
    /// resultado util.
    /// </summary>
    public static ComResto Pseudodividir(Polinomio dividendo, Polinomio divisor)
    {
        ArgumentNullException.ThrowIfNull(dividendo);
        ArgumentNullException.ThrowIfNull(divisor);
        if (divisor.EhZero) throw new DivideByZeroException("divisao por polinomio zero");

        if (dividendo.Grau < divisor.Grau) return new ComResto(Polinomio.Zero, dividendo);

        var lider = divisor[divisor.Grau];
        var vezes = dividendo.Grau - divisor.Grau + 1;

        var resto = dividendo.Vezes(BigInteger.Pow(lider, vezes));
        var quociente = Polinomio.Zero;

        while (!resto.EhZero && resto.Grau >= divisor.Grau)
        {
            var passo = resto.Grau - divisor.Grau;
            var fator = resto[resto.Grau] / lider;

            var termo = new Polinomio([fator]).Deslocado(passo);

            quociente += termo;
            resto -= Multiplicacao.Escolar(termo, divisor).Produto;
        }

        return new ComResto(quociente, resto);
    }

    /// <summary>
    /// A identidade que a pseudodivisao promete, para o teste conferir.
    /// </summary>
    public static Polinomio DividendoAjustado(Polinomio dividendo, Polinomio divisor)
    {
        ArgumentNullException.ThrowIfNull(dividendo);
        ArgumentNullException.ThrowIfNull(divisor);

        if (dividendo.Grau < divisor.Grau) return dividendo;

        var vezes = dividendo.Grau - divisor.Grau + 1;

        return dividendo.Vezes(BigInteger.Pow(divisor[divisor.Grau], vezes));
    }

    public sealed record Maximo(Polinomio Divisor, int Passos, int MaiorDigitos);

    /// <summary>
    /// O MAXIMO DIVISOR COMUM pelo algoritmo de Euclides, com pseudodivisao e sem
    /// nenhum cuidado.
    ///
    /// Ele e correto e e o jeito errado de fazer. A coluna de digitos no relatorio
    /// e o motivo: o maior coeficiente que aparece no caminho tem centenas de
    /// digitos, para polinomios de entrada com tres.
    /// </summary>
    public static Maximo MaximoIngenuo(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        var passos = 0;
        var maior = BigInteger.Max(a.MaiorCoeficiente(), b.MaiorCoeficiente());

        while (!b.EhZero)
        {
            var resto = Pseudodividir(a, b).Resto;

            a = b;
            b = resto;
            passos++;

            var atual = BigInteger.Max(a.MaiorCoeficiente(), b.MaiorCoeficiente());
            if (atual > maior) maior = atual;

            if (passos > 200) break;
        }

        return new Maximo(a, passos, maior.IsZero ? 1 : maior.ToString().Length);
    }

    /// <summary>
    /// O MESMO algoritmo, dividindo cada resto pelo conteudo dele.
    ///
    /// O conteudo e o maximo divisor comum dos coeficientes, e tirar ele nao muda
    /// o polinomio a menos de uma constante, que e tudo que importa num maximo
    /// divisor comum. Com esse unico cuidado, os coeficientes param de explodir.
    ///
    /// E uma linha a mais no laco, e a diferenca medida e de centenas de digitos
    /// para poucos.
    /// </summary>
    public static Maximo MaximoPrimitivo(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        a = SemConteudo(a);
        b = SemConteudo(b);

        var passos = 0;
        var maior = BigInteger.Max(a.MaiorCoeficiente(), b.MaiorCoeficiente());

        while (!b.EhZero)
        {
            var resto = SemConteudo(Pseudodividir(a, b).Resto);

            a = b;
            b = resto;
            passos++;

            var atual = BigInteger.Max(a.MaiorCoeficiente(), b.MaiorCoeficiente());
            if (atual > maior) maior = atual;

            if (passos > 200) break;
        }

        return new Maximo(a, passos, maior.IsZero ? 1 : maior.ToString().Length);
    }

    /// <summary>O conteudo: o maximo divisor comum dos coeficientes.</summary>
    public static BigInteger Conteudo(Polinomio p)
    {
        ArgumentNullException.ThrowIfNull(p);

        var conteudo = BigInteger.Zero;
        foreach (var c in p.Coeficientes) conteudo = BigInteger.GreatestCommonDivisor(conteudo, BigInteger.Abs(c));

        return conteudo.IsZero ? BigInteger.One : conteudo;
    }

    /// <summary>O polinomio dividido pelo proprio conteudo.</summary>
    public static Polinomio SemConteudo(Polinomio p)
    {
        ArgumentNullException.ThrowIfNull(p);
        if (p.EhZero) return p;

        var conteudo = Conteudo(p);

        return new Polinomio(p.Coeficientes.Select(c => c / conteudo));
    }

    /// <summary>
    /// Dois polinomios SORTEADOS com um fator comum conhecido, para a medida do
    /// maximo divisor comum ter uma resposta sabida de fora.
    /// </summary>
    public static (Polinomio A, Polinomio B, Polinomio Fator) ComFatorComum(int grau, int grauDoFator, Sorteio sorteio, int limite = 100)
    {
        ArgumentNullException.ThrowIfNull(sorteio);

        var fator = Polinomio.Sortear(grauDoFator, sorteio, limite);
        var a = Multiplicacao.Escolar(fator, Polinomio.Sortear(grau - grauDoFator, sorteio, limite)).Produto;
        var b = Multiplicacao.Escolar(fator, Polinomio.Sortear(grau - grauDoFator, sorteio, limite)).Produto;

        return (a, b, fator);
    }
}
