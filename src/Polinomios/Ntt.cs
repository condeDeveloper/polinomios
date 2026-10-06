using System.Numerics;

namespace Conde.Polinomios;

/// <summary>
/// A TRANSFORMADA NUMERICA: a transformada rapida de Fourier feita com inteiros
/// modulo um primo, sem um numero com virgula em lugar nenhum.
///
/// A ideia da transformada e trocar de representacao: um polinomio de grau menor
/// que n e determinado por n valores dele, e multiplicar NA REPRESENTACAO POR
/// VALORES e so multiplicar os valores um a um. O caro e ir e voltar entre as
/// duas representacoes, e a transformada faz isso em n log n em vez de n ao
/// quadrado, escolhendo os pontos com cuidado.
///
/// A versao com numeros complexos e a famosa, e ela tem um defeito que quase
/// nunca e dito em voz alta: ela ARREDONDA. O produto de dois polinomios de
/// coeficientes inteiros sai com coeficientes quase inteiros, e "quase" nao serve
/// quando o coeficiente certo tem vinte e tres digitos e a precisao dupla guarda
/// dezesseis.
///
/// Modulo um primo nao ha arredondamento nenhum: a resposta e exata ou nao sai.
/// E o preco e escolher o primo certo: ele precisa ter uma raiz da unidade de
/// ordem n, e e por isso que primos como 2^23 vezes 119 mais um aparecem em todo
/// codigo de competicao.
/// </summary>
public static class Ntt
{
    /// <summary>
    /// Um primo da forma k vezes 2^e mais um, com raiz primitiva conhecida.
    ///
    /// 2^23 vezes 119, mais um. Ele aceita transformadas de tamanho ate 2^23, e a
    /// raiz primitiva dele e 3.
    /// </summary>
    public const long Primo = 998244353;

    public const long RaizPrimitiva = 3;

    /// <summary>
    /// O PRODUTO de dois polinomios, com os coeficientes reduzidos modulo o primo.
    ///
    /// Repare no "reduzidos": a resposta so e o produto de verdade se todos os
    /// coeficientes do produto couberem abaixo do primo. Isso e uma condicao real
    /// e e facil de esquecer, e por isso ela e medida.
    /// </summary>
    public static Polinomio Multiplicar(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.EhZero || b.EhZero) return Polinomio.Zero;

        var tamanho = 1;
        while (tamanho < a.Termos + b.Termos - 1) tamanho <<= 1;

        var fa = ParaVetor(a, tamanho);
        var fb = ParaVetor(b, tamanho);

        Transformar(fa, inversa: false);
        Transformar(fb, inversa: false);

        for (var i = 0; i < tamanho; i++) fa[i] = fa[i] * fb[i] % Primo;

        Transformar(fa, inversa: true);

        // de volta para a faixa SIMETRICA, em torno do zero.
        //
        // Sem isso, um coeficiente de -600 sai como o primo menos 600, que e um
        // numero de nove digitos com cara de resposta. A conta modular esta certa
        // e a leitura dela, nao: os coeficientes de entrada tem sinal, e o resto
        // da divisao nao tem.
        return new Polinomio(fa
            .Take(a.Termos + b.Termos - 1)
            .Select(v => (BigInteger)(v > Primo / 2 ? v - Primo : v)));
    }

    private static long[] ParaVetor(Polinomio p, int tamanho)
    {
        var vetor = new long[tamanho];

        for (var i = 0; i < p.Termos; i++)
        {
            var resto = p[i] % Primo;
            if (resto.Sign < 0) resto += Primo;

            vetor[i] = (long)resto;
        }

        return vetor;
    }

    /// <summary>
    /// A transformada no lugar, pelo metodo de Cooley e Tukey.
    ///
    /// A inversao de bits no comeco e a mesma do algoritmo classico, e a unica
    /// diferenca para a versao com numeros complexos e de onde vem a raiz: la, do
    /// cosseno e do seno; aqui, de uma potencia da raiz primitiva do primo.
    /// </summary>
    public static void Transformar(long[] vetor, bool inversa)
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
            var raiz = Potencia(RaizPrimitiva, (Primo - 1) / tamanho);
            if (inversa) raiz = Inverso(raiz);

            for (var inicio = 0; inicio < n; inicio += tamanho)
            {
                long atual = 1;

                for (var k = 0; k < tamanho / 2; k++)
                {
                    var par = vetor[inicio + k];
                    var impar = vetor[inicio + k + tamanho / 2] * atual % Primo;

                    vetor[inicio + k] = (par + impar) % Primo;
                    vetor[inicio + k + tamanho / 2] = (par - impar + Primo) % Primo;

                    atual = atual * raiz % Primo;
                }
            }
        }

        if (!inversa) return;

        var inversoDeN = Inverso(n);
        for (var i = 0; i < n; i++) vetor[i] = vetor[i] * inversoDeN % Primo;
    }

    public static long Potencia(long baseDaConta, long expoente)
    {
        long resultado = 1;
        baseDaConta %= Primo;

        while (expoente > 0)
        {
            if ((expoente & 1) == 1) resultado = resultado * baseDaConta % Primo;

            baseDaConta = baseDaConta * baseDaConta % Primo;
            expoente >>= 1;
        }

        return resultado;
    }

    /// <summary>
    /// O inverso pelo pequeno teorema de Fermat: a elevado a p menos dois.
    ///
    /// So vale porque o modulo e PRIMO, e e mais um motivo de o primo nao ser
    /// escolhido a esmo.
    /// </summary>
    public static long Inverso(long valor) => Potencia(valor, Primo - 2);

    /// <summary>
    /// O produto cabe no primo, ou seja: a resposta modular e a resposta de
    /// verdade.
    ///
    /// Esta e a condicao que torna o resultado valido, e ela depende dos
    /// coeficientes E do grau: cada coeficiente do produto e uma soma de ate n
    /// produtos de coeficientes.
    /// </summary>
    public static bool CabeNoPrimo(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        var limite = BigInteger.Min(a.Termos, b.Termos) * a.MaiorCoeficiente() * b.MaiorCoeficiente();

        return limite < Primo / 2;
    }

    /// <summary>
    /// Quantas multiplicacoes a transformada gasta: tres transformadas de n log n
    /// mais n produtos ponto a ponto.
    /// </summary>
    public static long Multiplicacoes(int termosA, int termosB)
    {
        var tamanho = 1;
        while (tamanho < termosA + termosB - 1) tamanho <<= 1;

        var log = 0;
        while (1 << log < tamanho) log++;

        return 3L * tamanho * log / 2 + tamanho;
    }
}
