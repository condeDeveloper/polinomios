using System.Numerics;

namespace Conde.Polinomios;

/// <summary>
/// O JUIZ deste repositorio: dois polinomios sao iguais se eles CONCORDAM em
/// pontos suficientes.
///
/// O teorema e de duas linhas e e o mais util que existe sobre polinomios: um
/// polinomio nao nulo de grau d tem no maximo d raizes. Dai sai tudo:
///
///   - se dois polinomios de grau menor ou igual a d concordam em d+1 pontos
///     DISTINTOS, eles sao o MESMO polinomio. Nao "parecidos": o mesmo. A
///     diferenca entre eles tem grau no maximo d e tem d+1 raizes, entao ela e o
///     polinomio zero.
///
///   - se eles NAO sao iguais e o ponto e sorteado entre N valores, a chance de
///     eles concordarem por acaso e no maximo d sobre N.
///
/// E por isso que o juiz aqui nao e "comparar os vetores de coeficientes". Comparar
/// coeficientes diz que duas implementacoes fizeram a mesma coisa; avaliar em
/// pontos diz que as duas calcularam o POLINOMIO certo, e funciona ate quando as
/// duas representacoes nao sao comparaveis.
/// </summary>
public static class Identidade
{
    /// <summary>
    /// Os dois polinomios concordam em todos os pontos dados.
    /// </summary>
    public static bool ConcordamEm(Polinomio a, Polinomio b, IEnumerable<BigInteger> pontos)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        ArgumentNullException.ThrowIfNull(pontos);

        return pontos.All(ponto => a.Em(ponto) == b.Em(ponto));
    }

    /// <summary>
    /// O TESTE COMPLETO: concordar em grau+1 pontos distintos PROVA a igualdade.
    ///
    /// Nao e probabilistico, nao tem margem: e o teorema. Por isso ele vale como
    /// juiz de verdade e nao so como uma conferencia barata.
    /// </summary>
    public static bool SaoIguais(Polinomio a, Polinomio b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        var grau = Math.Max(a.Grau, b.Grau);
        if (grau < 0) return true;

        var pontos = Enumerable.Range(0, grau + 1).Select(i => (BigInteger)(i + 1));

        return ConcordamEm(a, b, pontos);
    }

    /// <summary>
    /// O TESTE SORTEADO: um punhado de pontos, e a chance de errar.
    ///
    /// Com um ponto sorteado entre N valores, dois polinomios diferentes de grau
    /// no maximo d passam com chance no maximo d/N. Com k pontos independentes, a
    /// chance cai para (d/N) elevado a k.
    ///
    /// Com grau mil e um milhao de valores possiveis, um ponto so ja da uma chance
    /// de um em mil de passar errado; tres pontos dao um em um bilhao. E por isso
    /// que testar identidade de polinomio por sorteio e um dos algoritmos
    /// probabilisticos mais usados que existem.
    /// </summary>
    public static bool ProvavelmenteIguais(Polinomio a, Polinomio b, Sorteio sorteio, int pontos = 3, int faixa = 1_000_000)
    {
        ArgumentNullException.ThrowIfNull(sorteio);

        for (var i = 0; i < pontos; i++)
        {
            var ponto = (BigInteger)sorteio.Entre(1, faixa);
            if (a.Em(ponto) != b.Em(ponto)) return false;
        }

        return true;
    }

    /// <summary>
    /// A chance maxima de o teste sorteado errar, pela conta do teorema.
    /// </summary>
    public static double ChanceDeErrar(int grau, int pontos, int faixa) =>
        Math.Pow((double)grau / faixa, pontos);

    /// <summary>
    /// A medida que mostra o teorema funcionando: quantos pontos sorteados sao
    /// precisos para pegar uma diferenca de verdade.
    ///
    /// Devolve em quantos dos sorteios o primeiro ponto ja pegou a diferenca. Com
    /// a faixa grande, a resposta e "quase sempre", e e isso que o teorema promete.
    /// </summary>
    public static int PrimeiroPontoJaPegou(Polinomio a, Polinomio b, Sorteio sorteio, int tentativas, int faixa)
    {
        ArgumentNullException.ThrowIfNull(sorteio);

        var pegou = 0;

        for (var i = 0; i < tentativas; i++)
        {
            var ponto = (BigInteger)sorteio.Entre(1, faixa);
            if (a.Em(ponto) != b.Em(ponto)) pegou++;
        }

        return pegou;
    }

    /// <summary>
    /// Quantas RAIZES o polinomio tem numa faixa de inteiros, contadas por forca
    /// bruta.
    ///
    /// Ela existe para o teorema nao ser so uma frase: um polinomio de grau d
    /// nunca tem mais que d raizes, e o teste varre a faixa e confere.
    /// </summary>
    public static int RaizesInteirasAte(Polinomio p, int limite)
    {
        ArgumentNullException.ThrowIfNull(p);

        var quantas = 0;

        for (var x = -limite; x <= limite; x++)
            if (p.Em(x).IsZero) quantas++;

        return quantas;
    }
}
