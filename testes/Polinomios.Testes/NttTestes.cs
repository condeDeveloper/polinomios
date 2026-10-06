using System.Numerics;
using Conde.Polinomios;
using Xunit;

namespace Conde.Polinomios.Testes;

public class NttTestes
{
    /// <summary>
    /// A TRANSFORMADA E A INVERSA se desfazem, que e a propriedade que faz o
    /// metodo existir.
    /// </summary>
    [Fact]
    public void ATransformadaEAInversaSeDesfazem()
    {
        var sorteio = new Sorteio(3);

        foreach (var tamanho in new[] { 1, 2, 4, 8, 16, 64, 256 })
            for (var caso = 0; caso < 20; caso++)
            {
                var original = new long[tamanho];
                for (var i = 0; i < tamanho; i++) original[i] = sorteio.Entre(0, 100_000);

                var copia = (long[])original.Clone();

                Ntt.Transformar(copia, inversa: false);
                Ntt.Transformar(copia, inversa: true);

                Assert.Equal(original, copia);
            }
    }

    /// <summary>
    /// A RAIZ PRIMITIVA e mesmo uma raiz da unidade de ordem n: elevada a n da um,
    /// e elevada a n/2 nao da.
    ///
    /// Sem isso a transformada nao separa nada, e a conta sai um numero qualquer.
    /// </summary>
    [Fact]
    public void ARaizEhMesmoDaOrdemCerta()
    {
        foreach (var tamanho in new[] { 2, 4, 8, 1024, 1 << 20 })
        {
            var raiz = Ntt.Potencia(Ntt.RaizPrimitiva, (Ntt.Primo - 1) / tamanho);

            Assert.Equal(1, Ntt.Potencia(raiz, tamanho));
            Assert.NotEqual(1, Ntt.Potencia(raiz, tamanho / 2));
        }
    }

    /// <summary>
    /// O INVERSO pelo pequeno teorema de Fermat: so vale porque o modulo e primo,
    /// e e mais um motivo de o primo nao ser escolhido a esmo.
    /// </summary>
    [Fact]
    public void OInversoDesfazAMultiplicacao()
    {
        var sorteio = new Sorteio(5);

        for (var caso = 0; caso < 1_000; caso++)
        {
            var valor = sorteio.Entre(1, 1_000_000);

            Assert.Equal(1, valor * Ntt.Inverso(valor) % Ntt.Primo);
        }
    }

    /// <summary>
    /// O PRODUTO pela transformada e exatamente o produto escolar, sempre que ele
    /// cabe no primo.
    /// </summary>
    [Fact]
    public void OProdutoPelaTransformadaEhOProdutoDeVerdade()
    {
        var sorteio = new Sorteio(7);
        var conferidos = 0;

        for (var caso = 0; caso < 500; caso++)
        {
            var a = Polinomio.Sortear(sorteio.Entre(0, 60), sorteio, 50);
            var b = Polinomio.Sortear(sorteio.Entre(0, 60), sorteio, 50);

            if (!Ntt.CabeNoPrimo(a, b)) continue;

            conferidos++;
            Assert.Equal(Multiplicacao.Escolar(a, b).Produto, Ntt.Multiplicar(a, b));
        }

        Assert.True(conferidos > 300, $"so {conferidos} casos couberam no primo");
    }

    /// <summary>
    /// E ele acerta com coeficientes NEGATIVOS, que e onde a primeira versao daqui
    /// errava.
    ///
    /// A conta modular estava certa e a leitura dela, nao: um coeficiente de -600
    /// saia como o primo menos 600, que e um numero de nove digitos com cara de
    /// resposta. O conserto e devolver os coeficientes na faixa simetrica em torno
    /// do zero.
    /// </summary>
    [Fact]
    public void ElePrecisaDevolverOsCoeficientesComSinal()
    {
        var a = Polinomio.De(-3, 5);
        var b = Polinomio.De(2, -7);

        var produto = Ntt.Multiplicar(a, b);

        Assert.Equal(Multiplicacao.Escolar(a, b).Produto, produto);
        Assert.Equal(-6, produto[0]);
        Assert.True(produto.Coeficientes.Any(c => c.Sign < 0));
    }

    /// <summary>
    /// QUANDO NAO CABE no primo, a resposta deixa de ser o produto, e isso e uma
    /// condicao real e facil de esquecer.
    ///
    /// Com coeficientes grandes, cada coeficiente do produto passa do primo e o
    /// resto da divisao e o que sai. Nao ha erro, nao ha excecao: ha outro
    /// polinomio.
    /// </summary>
    [Fact]
    public void QuandoNaoCabeNoPrimoARespostaDeixaDeSerOProduto()
    {
        var sorteio = new Sorteio(11);

        var a = Polinomio.Sortear(255, sorteio, 1_000_000);
        var b = Polinomio.Sortear(255, sorteio, 1_000_000);

        Assert.False(Ntt.CabeNoPrimo(a, b));
        Assert.NotEqual(Multiplicacao.Escolar(a, b).Produto, Ntt.Multiplicar(a, b));

        // e mesmo errada, ela continua certa MODULO o primo
        var certo = Multiplicacao.Escolar(a, b).Produto;
        var modular = Ntt.Multiplicar(a, b);

        for (var i = 0; i < certo.Termos; i++)
            Assert.Equal(((certo[i] % Ntt.Primo) + Ntt.Primo) % Ntt.Primo, ((modular[i] % Ntt.Primo) + Ntt.Primo) % Ntt.Primo);
    }

    /// <summary>
    /// A contagem de multiplicacoes da transformada e n log n, e ela ganha da
    /// escolar bem antes do que a intuicao diz: ja no grau 31.
    /// </summary>
    [Fact]
    public void ATransformadaGanhaDaEscolarCedo()
    {
        Assert.True(Ntt.Multiplicacoes(32, 32) < Multiplicacao.PreditoEscolar(32));
        Assert.True(Ntt.Multiplicacoes(16, 16) > Multiplicacao.PreditoEscolar(16) / 2);

        // e com grau mil ela gasta vinte e nove vezes menos
        var razao = Multiplicacao.PreditoEscolar(1024) / Ntt.Multiplicacoes(1024, 1024);
        Assert.InRange(razao, 20, 40);
    }

    [Fact]
    public void MultiplicarPorZeroDaZero()
    {
        Assert.True(Ntt.Multiplicar(Polinomio.De(1, 2, 3), Polinomio.Zero).EhZero);
        Assert.True(Ntt.Multiplicar(Polinomio.Zero, Polinomio.De(1, 2, 3)).EhZero);
    }

    [Fact]
    public void MultiplicarPorUmDevolveOMesmo()
    {
        var sorteio = new Sorteio(13);

        for (var caso = 0; caso < 100; caso++)
        {
            var p = Polinomio.Sortear(sorteio.Entre(0, 30), sorteio, 100);

            Assert.Equal(p, Ntt.Multiplicar(p, Polinomio.Um));
        }
    }
}
