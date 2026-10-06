using System.Numerics;
using Conde.Polinomios;
using Xunit;

namespace Conde.Polinomios.Testes;

public class MultiplicacaoTestes
{
    [Fact]
    public void OProdutoDeCasosConhecidos()
    {
        // (x + 1)(x - 1) = x^2 - 1
        Assert.Equal(Polinomio.De(-1, 0, 1), Multiplicacao.Escolar(Polinomio.De(1, 1), Polinomio.De(-1, 1)).Produto);

        // (x + 1)^2 = x^2 + 2x + 1
        Assert.Equal(Polinomio.De(1, 2, 1), Multiplicacao.Escolar(Polinomio.De(1, 1), Polinomio.De(1, 1)).Produto);

        // qualquer coisa vezes zero e zero
        Assert.True(Multiplicacao.Escolar(Polinomio.De(1, 2, 3), Polinomio.Zero).Produto.EhZero);
    }

    /// <summary>
    /// O GRAU do produto e a soma dos graus, sempre, porque os coeficientes
    /// lideres sao nao nulos e o produto deles tambem.
    /// </summary>
    [Fact]
    public void OGrauDoProdutoEhASomaDosGraus()
    {
        var sorteio = new Sorteio(3);

        for (var caso = 0; caso < 300; caso++)
        {
            var a = Polinomio.Sortear(sorteio.Entre(0, 20), sorteio);
            var b = Polinomio.Sortear(sorteio.Entre(0, 20), sorteio);

            Assert.Equal(a.Grau + b.Grau, Multiplicacao.Escolar(a, b).Produto.Grau);
        }
    }

    /// <summary>
    /// O JUIZ aplicado a multiplicacao: o produto avaliado em x e o produto das
    /// avaliacoes. Isso confere a conta inteira sem olhar um coeficiente.
    /// </summary>
    [Fact]
    public void OProdutoAvaliadoEhOProdutoDasAvaliacoes()
    {
        var sorteio = new Sorteio(5);

        for (var caso = 0; caso < 300; caso++)
        {
            var a = Polinomio.Sortear(sorteio.Entre(0, 15), sorteio, 100);
            var b = Polinomio.Sortear(sorteio.Entre(0, 15), sorteio, 100);
            var produto = Multiplicacao.Escolar(a, b).Produto;

            var x = (BigInteger)sorteio.Entre(-20, 20);

            Assert.Equal(a.Em(x) * b.Em(x), produto.Em(x));
        }
    }

    /// <summary>
    /// KARATSUBA da exatamente o mesmo polinomio que a escolar, em qualquer corte,
    /// em qualquer grau.
    /// </summary>
    [Fact]
    public void OKaratsubaDaOMesmoProdutoQueAEscolar()
    {
        var sorteio = new Sorteio(7);

        foreach (var corte in new[] { 1, 2, 4, 8, 32 })
            for (var caso = 0; caso < 60; caso++)
            {
                var a = Polinomio.Sortear(sorteio.Entre(0, 60), sorteio, 200);
                var b = Polinomio.Sortear(sorteio.Entre(0, 60), sorteio, 200);

                var escolar = Multiplicacao.Escolar(a, b).Produto;
                var karatsuba = Multiplicacao.Karatsuba(a, b, corte).Produto;

                Assert.True(Identidade.SaoIguais(escolar, karatsuba), $"corte {corte}, caso {caso}");
                Assert.Equal(escolar, karatsuba);
            }
    }

    [Fact]
    public void OKaratsubaAguentaPolinomiosDeTamanhosMuitoDiferentes()
    {
        var sorteio = new Sorteio(9);

        for (var caso = 0; caso < 50; caso++)
        {
            var a = Polinomio.Sortear(sorteio.Entre(0, 5), sorteio);
            var b = Polinomio.Sortear(sorteio.Entre(60, 120), sorteio);

            Assert.Equal(Multiplicacao.Escolar(a, b).Produto, Multiplicacao.Karatsuba(a, b, 4).Produto);
            Assert.Equal(Multiplicacao.Escolar(b, a).Produto, Multiplicacao.Karatsuba(b, a, 4).Produto);
        }
    }

    /// <summary>
    /// A CONTAGEM da escolar e exatamente o produto dos numeros de termos, sem
    /// aproximacao.
    /// </summary>
    [Fact]
    public void AContagemDaEscolarEhExata()
    {
        var sorteio = new Sorteio(11);

        for (var caso = 0; caso < 100; caso++)
        {
            var a = Polinomio.Sortear(sorteio.Entre(0, 30), sorteio);
            var b = Polinomio.Sortear(sorteio.Entre(0, 30), sorteio);

            Assert.Equal((long)a.Termos * b.Termos, Multiplicacao.Escolar(a, b).Multiplicacoes);
        }
    }

    /// <summary>
    /// O ACHADO QUE A NOTACAO ESCONDE: com grau pequeno, o Karatsuba gasta o
    /// MESMO que a escolar.
    ///
    /// Com grau 15 e corte 32, a recursao nem acontece: o Karatsuba E a escolar.
    /// A vantagem so aparece a partir de grau algumas dezenas, e e por isso que
    /// toda implementacao de verdade tem um corte.
    /// </summary>
    [Fact]
    public void ComGrauPequenoOKaratsubaEhAEscolar()
    {
        var sorteio = new Sorteio(13);

        var pequeno = Polinomio.Sortear(15, sorteio);
        var outro = Polinomio.Sortear(15, sorteio);

        Assert.Equal(
            Multiplicacao.Escolar(pequeno, outro).Multiplicacoes,
            Multiplicacao.Karatsuba(pequeno, outro, 32).Multiplicacoes);
    }

    /// <summary>
    /// E com grau mil, ele gasta um quarto: um milhao de multiplicacoes contra
    /// duzentas e quarenta e oito mil.
    /// </summary>
    [Fact]
    public void ComGrauMilOKaratsubaGastaUmQuarto()
    {
        var sorteio = new Sorteio(7);

        var a = Polinomio.Sortear(1023, sorteio);
        var b = Polinomio.Sortear(1023, sorteio);

        var escolar = Multiplicacao.Escolar(a, b).Multiplicacoes;
        var karatsuba = Multiplicacao.Karatsuba(a, b).Multiplicacoes;

        Assert.Equal(1_048_576, escolar);
        Assert.InRange((double)escolar / karatsuba, 3.5, 5.0);
    }

    /// <summary>
    /// A conta fechada e um TETO, e a contagem medida fica um pouco abaixo dele.
    ///
    /// Eu tinha escrito "bate" e esperado igualdade exata. A diferenca e pequena
    /// (248.800 contra 248.832, com grau 1023) e tem motivo: a soma das duas
    /// metades as vezes CANCELA o coeficiente de maior grau, o polinomio fica com
    /// um termo a menos e a recursao daquele ramo gasta um pouco menos. A conta
    /// fechada nao sabe disso porque ela so olha tamanhos.
    /// </summary>
    [Fact]
    public void AContaFechadaEhUmTetoEAContagemFicaAbaixo()
    {
        var sorteio = new Sorteio(7);

        foreach (var grau in new[] { 63, 127, 255, 511, 1023 })
        {
            var a = Polinomio.Sortear(grau, sorteio);
            var b = Polinomio.Sortear(grau, sorteio);

            Assert.Equal(Multiplicacao.PreditoEscolar(a.Termos), Multiplicacao.Escolar(a, b).Multiplicacoes, 6);

            var medido = Multiplicacao.Karatsuba(a, b).Multiplicacoes;
            var teto = Multiplicacao.PreditoKaratsuba(a.Termos);

            Assert.True(medido <= teto, $"grau {grau}: medido {medido}, teto {teto}");
            Assert.True(medido > teto * 0.99, $"grau {grau}: medido {medido} ficou muito abaixo do teto {teto}");
        }
    }

    /// <summary>
    /// A multiplicacao e COMUTATIVA e ASSOCIATIVA, e isso nao e obvio na
    /// implementacao recursiva: o Karatsuba parte os dois lados em lugares
    /// diferentes dependendo de qual e maior.
    /// </summary>
    [Fact]
    public void OProdutoEhComutativoEAssociativo()
    {
        var sorteio = new Sorteio(17);

        for (var caso = 0; caso < 100; caso++)
        {
            var a = Polinomio.Sortear(sorteio.Entre(1, 30), sorteio, 50);
            var b = Polinomio.Sortear(sorteio.Entre(1, 30), sorteio, 50);
            var c = Polinomio.Sortear(sorteio.Entre(1, 30), sorteio, 50);

            Assert.Equal(Multiplicacao.Karatsuba(a, b, 4).Produto, Multiplicacao.Karatsuba(b, a, 4).Produto);

            var esquerda = Multiplicacao.Karatsuba(Multiplicacao.Karatsuba(a, b, 4).Produto, c, 4).Produto;
            var direita = Multiplicacao.Karatsuba(a, Multiplicacao.Karatsuba(b, c, 4).Produto, 4).Produto;

            Assert.Equal(esquerda, direita);
        }
    }
}
