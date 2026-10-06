using System.Numerics;

namespace Conde.Polinomios;

/// <summary>
/// As tres MULTIPLICACOES, e o que separa elas.
///
/// Multiplicar polinomio e o problema que mostra melhor que algoritmo nao e
/// detalhe de implementacao: as tres daqui dao exatamente o mesmo resultado, com
/// os mesmos inteiros, e gastam n ao quadrado, n elevado a 1,585 e n log n
/// multiplicacoes. Com grau mil isso e um milhao, oitenta mil e dez mil.
///
/// A contagem e de MULTIPLICACOES DE COEFICIENTE, nunca de segundos. O numero e o
/// mesmo nos tres sistemas e nao depende de quanto o processador estava ocupado.
/// </summary>
public static class Multiplicacao
{
    /// <summary>Quantas multiplicacoes de coeficiente a ultima conta gastou.</summary>
    public sealed record Resultado(Polinomio Produto, long Multiplicacoes);

    /// <summary>
    /// A ESCOLAR: cada termo vezes cada termo.
    ///
    /// Ela e o que todo mundo escreve, e ela e a MELHOR das tres ate um grau
    /// surpreendentemente alto. A constante dela e um, e as outras duas pagam
    /// somas, copias e chamadas recursivas que nao aparecem na notacao assintotica.
    /// </summary>
    public static Resultado Escolar(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.EhZero || b.EhZero) return new Resultado(Polinomio.Zero, 0);

        var produto = new BigInteger[a.Termos + b.Termos - 1];
        long contas = 0;

        for (var i = 0; i < a.Termos; i++)
            for (var j = 0; j < b.Termos; j++)
            {
                produto[i + j] += a[i] * b[j];
                contas++;
            }

        return new Resultado(new Polinomio(produto), contas);
    }

    /// <summary>
    /// KARATSUBA, 1960: tres multiplicacoes no lugar de quatro.
    ///
    /// Partindo cada polinomio em duas metades, o produto precisa de quatro
    /// produtos de metade: alta vezes alta, alta vezes baixa, baixa vezes alta e
    /// baixa vezes baixa. Karatsuba notou que os dois do meio sao a diferenca
    /// entre (alta + baixa) ao quadrado e os outros dois, entao bastam TRES.
    ///
    /// Tres em vez de quatro, repetido, troca n ao quadrado por n elevado a
    /// log 3 na base 2, que e 1,585. Nao parece muito e e: com grau mil, e doze
    /// vezes menos multiplicacoes.
    ///
    /// E a historia por tras disso vale: Kolmogorov tinha conjecturado que n ao
    /// quadrado era o minimo, e Karatsuba achou isto em uma semana, com vinte e
    /// tres anos.
    /// </summary>
    public static Resultado Karatsuba(Polinomio a, Polinomio b, int corte = 32)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        long contas = 0;
        var produto = Recursivo(a, b, corte, ref contas);

        return new Resultado(produto, contas);
    }

    private static Polinomio Recursivo(Polinomio a, Polinomio b, int corte, ref long contas)
    {
        if (a.EhZero || b.EhZero) return Polinomio.Zero;

        if (a.Termos <= corte || b.Termos <= corte)
        {
            var escolar = Escolar(a, b);
            contas += escolar.Multiplicacoes;

            return escolar.Produto;
        }

        var meio = Math.Max(a.Termos, b.Termos) / 2;

        var a0 = a.ParteBaixa(meio);
        var a1 = a.ParteAlta(meio);
        var b0 = b.ParteBaixa(meio);
        var b1 = b.ParteAlta(meio);

        var baixo = Recursivo(a0, b0, corte, ref contas);
        var alto = Recursivo(a1, b1, corte, ref contas);
        var meioTermo = Recursivo(a0 + a1, b0 + b1, corte, ref contas) - baixo - alto;

        return baixo + meioTermo.Deslocado(meio) + alto.Deslocado(2 * meio);
    }

    /// <summary>
    /// A conta fechada de quantas multiplicacoes cada metodo gasta, para comparar
    /// com o medido.
    /// </summary>
    public static double PreditoEscolar(int termos) => (double)termos * termos;

    public static double PreditoKaratsuba(int termos, int corte = 32)
    {
        if (termos <= corte) return (double)termos * termos;

        return 3 * PreditoKaratsuba((termos + 1) / 2, corte);
    }
}
