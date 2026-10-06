using System.Numerics;

namespace Conde.Polinomios;

/// <summary>
/// A TRANSFORMADA COM NUMEROS COMPLEXOS, que e a famosa, e o defeito dela.
///
/// Ela e o algoritmo que todo mundo aprende: cosseno, seno, raizes da unidade no
/// circulo. E ela multiplica polinomios de coeficientes inteiros devolvendo
/// numeros com virgula que PRECISAM ser arredondados no fim.
///
/// Enquanto os coeficientes sao pequenos, o arredondamento acerta e ninguem
/// percebe. A partir de um certo tamanho ele erra, e o erro nao e um alerta: e um
/// coeficiente diferente, num vetor de coeficientes certos.
///
/// Esta classe existe para medir onde isso comeca. A resposta surpreende: nao e
/// preciso ir longe.
/// </summary>
public static class Flutuante
{
    /// <summary>
    /// O produto pela transformada com numeros complexos, arredondado no fim.
    /// </summary>
    public static Polinomio Multiplicar(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.EhZero || b.EhZero) return Polinomio.Zero;

        var precisa = a.Termos + b.Termos - 1;
        var tamanho = 1;
        while (tamanho < precisa) tamanho <<= 1;

        var fa = ParaComplexos(a, tamanho);
        var fb = ParaComplexos(b, tamanho);

        Transformar(fa, inversa: false);
        Transformar(fb, inversa: false);

        for (var i = 0; i < tamanho; i++) fa[i] *= fb[i];

        Transformar(fa, inversa: true);

        var coeficientes = new BigInteger[precisa];
        for (var i = 0; i < precisa; i++) coeficientes[i] = new BigInteger(Math.Round(fa[i].Real));

        return new Polinomio(coeficientes);
    }

    /// <summary>
    /// O MAIOR DESVIO entre o valor com virgula e o inteiro mais proximo.
    ///
    /// Esta e a medida que importa: enquanto ele fica bem abaixo de meio, o
    /// arredondamento acerta. Quando ele chega perto de meio, alguma coisa ja esta
    /// errada, e o produto continua com cara de resposta.
    /// </summary>
    public static double MaiorDesvio(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.EhZero || b.EhZero) return 0;

        var precisa = a.Termos + b.Termos - 1;
        var tamanho = 1;
        while (tamanho < precisa) tamanho <<= 1;

        var fa = ParaComplexos(a, tamanho);
        var fb = ParaComplexos(b, tamanho);

        Transformar(fa, inversa: false);
        Transformar(fb, inversa: false);

        for (var i = 0; i < tamanho; i++) fa[i] *= fb[i];

        Transformar(fa, inversa: true);

        var maior = 0.0;
        for (var i = 0; i < precisa; i++)
        {
            var desvio = Math.Abs(fa[i].Real - Math.Round(fa[i].Real));
            if (desvio > maior) maior = desvio;
        }

        return maior;
    }

    private static Complex[] ParaComplexos(Polinomio p, int tamanho)
    {
        var vetor = new Complex[tamanho];
        for (var i = 0; i < p.Termos; i++) vetor[i] = new Complex((double)p[i], 0);

        return vetor;
    }

    /// <summary>A transformada no lugar, pelo metodo de Cooley e Tukey.</summary>
    public static void Transformar(Complex[] vetor, bool inversa)
    {
        ArgumentNullException.ThrowIfNull(vetor);

        var n = vetor.Length;
        if (n <= 1) return;

        for (int i = 1, j = 0; i < n; i++)
        {
            var bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;

            if (i < j) (vetor[i], vetor[j]) = (vetor[j], vetor[i]);
        }

        for (var tamanho = 2; tamanho <= n; tamanho <<= 1)
        {
            var angulo = 2 * Math.PI / tamanho * (inversa ? 1 : -1);
            var raiz = new Complex(Math.Cos(angulo), Math.Sin(angulo));

            for (var inicio = 0; inicio < n; inicio += tamanho)
            {
                var atual = Complex.One;

                for (var k = 0; k < tamanho / 2; k++)
                {
                    var par = vetor[inicio + k];
                    var impar = vetor[inicio + k + tamanho / 2] * atual;

                    vetor[inicio + k] = par + impar;
                    vetor[inicio + k + tamanho / 2] = par - impar;

                    atual *= raiz;
                }
            }
        }

        if (!inversa) return;

        for (var i = 0; i < n; i++) vetor[i] /= n;
    }

    /// <summary>
    /// A conta de quanto o maior coeficiente do produto pode valer, que e o que
    /// decide se a precisao dupla aguenta.
    ///
    /// Cada coeficiente do produto e uma soma de ate n produtos de coeficientes,
    /// entao ele nao passa de n vezes o maior de um vezes o maior do outro. A
    /// precisao dupla guarda 53 bits de mantissa, ou seja, inteiros exatos ate
    /// uns nove mil trilhoes.
    /// </summary>
    public static BigInteger MaiorCoeficientePossivel(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        return BigInteger.Min(a.Termos, b.Termos) * a.MaiorCoeficiente() * b.MaiorCoeficiente();
    }

    /// <summary>O maior inteiro que a precisao dupla representa sem pular nenhum.</summary>
    public static readonly BigInteger LimiteDaPrecisaoDupla = BigInteger.Pow(2, 53);
}
