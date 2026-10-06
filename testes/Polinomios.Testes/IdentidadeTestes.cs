using System.Numerics;
using Conde.Polinomios;
using Xunit;

namespace Conde.Polinomios.Testes;

/// <summary>
/// O juiz tambem precisa de juiz.
///
/// Todo veredicto deste repositorio e "os dois polinomios concordam em pontos
/// suficientes". Se o teste de identidade tivesse um erro, duas multiplicacoes
/// diferentes passariam por iguais. Por isso ele e conferido contra o teorema das
/// raizes, contra casos de resposta conhecida e contra a comparacao direta de
/// coeficientes.
/// </summary>
public class IdentidadeTestes
{
    /// <summary>
    /// O TEOREMA: um polinomio nao nulo de grau d tem no maximo d raizes.
    ///
    /// Conferido por forca bruta numa faixa larga de inteiros, em muitos
    /// polinomios sorteados. E dele que sai tudo o mais.
    /// </summary>
    [Fact]
    public void UmPolinomioDeGrauDTemNoMaximoDRaizes()
    {
        var sorteio = new Sorteio(3);

        for (var caso = 0; caso < 300; caso++)
        {
            var grau = sorteio.Entre(1, 8);
            var p = Polinomio.Sortear(grau, sorteio, limite: 30);

            Assert.True(Identidade.RaizesInteirasAte(p, 200) <= grau, $"grau {grau}: {p}");
        }
    }

    /// <summary>
    /// E um polinomio montado com d raizes conhecidas tem EXATAMENTE d.
    /// </summary>
    [Fact]
    public void UmPolinomioMontadoComDRaizesTemExatamenteD()
    {
        for (var grau = 1; grau <= 7; grau++)
        {
            var p = Polinomio.Um;
            for (var raiz = 1; raiz <= grau; raiz++) p = Multiplicacao.Escolar(p, Polinomio.De(-raiz, 1)).Produto;

            Assert.Equal(grau, p.Grau);
            Assert.Equal(grau, Identidade.RaizesInteirasAte(p, 100));
        }
    }

    /// <summary>
    /// CONCORDAR EM GRAU+1 PONTOS DISTINTOS E A IGUALDADE. Nao e evidencia dela: e
    /// ela.
    /// </summary>
    [Fact]
    public void ConcordarEmGrauMaisUmPontosEhAIgualdade()
    {
        var sorteio = new Sorteio(5);

        for (var caso = 0; caso < 300; caso++)
        {
            var a = Polinomio.Sortear(sorteio.Entre(0, 12), sorteio, 100);
            var b = Polinomio.Sortear(sorteio.Entre(0, 12), sorteio, 100);

            Assert.Equal(a.Equals(b), Identidade.SaoIguais(a, b));
        }
    }

    /// <summary>
    /// E concordar em MENOS pontos que o grau NAO basta: dois polinomios
    /// diferentes de grau d concordam em ate d pontos.
    /// </summary>
    [Fact]
    public void ConcordarEmPoucosPontosNaoBasta()
    {
        // (x-1)(x-2)(x-3) e zero em 1, 2 e 3, e nao e o polinomio zero
        var diferenca = Multiplicacao.Escolar(
            Multiplicacao.Escolar(Polinomio.De(-1, 1), Polinomio.De(-2, 1)).Produto,
            Polinomio.De(-3, 1)).Produto;

        Assert.True(Identidade.ConcordamEm(diferenca, Polinomio.Zero, [1, 2, 3]));
        Assert.False(diferenca.EhZero);
        Assert.False(Identidade.SaoIguais(diferenca, Polinomio.Zero));
    }

    /// <summary>
    /// O TESTE SORTEADO erra com chance no maximo grau sobre faixa, e a medida
    /// bate com a conta no PIOR CASO possivel: a diferenca com o maior numero de
    /// raizes que o grau permite.
    /// </summary>
    [Fact]
    public void OTesteSorteadoErraDentroDoQueOTeoremaPermite()
    {
        var sorteio = new Sorteio(7);

        foreach (var (grau, faixa) in new[] { (10, 20), (10, 100), (20, 100) })
        {
            var a = Polinomio.Sortear(grau, sorteio, 20);
            var diferenca = Polinomio.Um;

            for (var raiz = 1; raiz <= grau; raiz++)
                diferenca = Multiplicacao.Escolar(diferenca, Polinomio.De(-raiz, 1)).Produto;

            var b = a + diferenca;
            var pegou = Identidade.PrimeiroPontoJaPegou(a, b, sorteio, 10_000, faixa);
            var falhou = 1 - (double)pegou / 10_000;

            Assert.True(falhou <= Identidade.ChanceDeErrar(grau, 1, faixa) + 0.02,
                $"grau {grau}, faixa {faixa}: falhou {falhou:P2}, o teorema permite {Identidade.ChanceDeErrar(grau, 1, faixa):P2}");
        }
    }

    /// <summary>
    /// Com a faixa grande, o teste sorteado com tres pontos nao erra nunca em dez
    /// mil tentativas, e a conta diz por que: um em um bilhao.
    /// </summary>
    [Fact]
    public void ComAFaixaGrandeOTesteSorteadoNaoErra()
    {
        var sorteio = new Sorteio(11);

        for (var caso = 0; caso < 2_000; caso++)
        {
            var a = Polinomio.Sortear(sorteio.Entre(1, 20), sorteio, 100);
            var b = a + Polinomio.De(1).Deslocado(sorteio.Entre(0, 5));

            Assert.False(Identidade.ProvavelmenteIguais(a, b, sorteio, pontos: 3));
        }

        Assert.Equal(1e-9, Identidade.ChanceDeErrar(1000, 3, 1_000_000), 15);
    }

    [Fact]
    public void PolinomiosIguaisSempreConcordam()
    {
        var sorteio = new Sorteio(13);

        for (var caso = 0; caso < 300; caso++)
        {
            var p = Polinomio.Sortear(sorteio.Entre(0, 20), sorteio);

            Assert.True(Identidade.SaoIguais(p, p));
            Assert.True(Identidade.ProvavelmenteIguais(p, p, sorteio, 5));
        }
    }

    [Fact]
    public void OPolinomioZeroEhIgualAoPolinomioZero()
    {
        Assert.True(Identidade.SaoIguais(Polinomio.Zero, Polinomio.Zero));
        Assert.True(Identidade.SaoIguais(Polinomio.Zero, Polinomio.De(0, 0)));
        Assert.False(Identidade.SaoIguais(Polinomio.Zero, Polinomio.Um));
    }

    /// <summary>
    /// A conta da chance cai como uma potencia no numero de pontos, e e isso que
    /// torna o teste utilizavel de verdade.
    /// </summary>
    [Fact]
    public void AChanceCaiComoUmaPotenciaNoNumeroDePontos()
    {
        var um = Identidade.ChanceDeErrar(100, 1, 1000);
        var dois = Identidade.ChanceDeErrar(100, 2, 1000);
        var tres = Identidade.ChanceDeErrar(100, 3, 1000);

        Assert.Equal(0.1, um, 12);
        Assert.Equal(um * um, dois, 12);
        Assert.Equal(um * um * um, tres, 12);
    }
}
