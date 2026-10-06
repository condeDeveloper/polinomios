using System.Numerics;
using Conde.Polinomios;
using Xunit;

namespace Conde.Polinomios.Testes;

public class PolinomioTestes
{
    [Fact]
    public void OSorteioEhDeterminista()
    {
        var a = new Sorteio(7);
        var b = new Sorteio(7);

        for (var i = 0; i < 1000; i++) Assert.Equal(a.Proximo(), b.Proximo());
    }

    /// <summary>
    /// A fracao precisa cobrir os DOIS LADOS do meio: um deslocamento de 33 em vez
    /// de 32 deixaria o gerador com 31 bits e metade das escolhas sorteadas
    /// deixaria de acontecer, com os numeros continuando plausiveis.
    /// </summary>
    [Fact]
    public void AFracaoCobreOsDoisLadosDoMeio()
    {
        var sorteio = new Sorteio(7);
        var acima = 0;
        var maior = 0.0;

        for (var i = 0; i < 10_000; i++)
        {
            var f = sorteio.Fracao();

            Assert.InRange(f, 0.0, 1.0);
            if (f > 0.5) acima++;
            if (f > maior) maior = f;
        }

        Assert.InRange(acima, 4_700, 5_300);
        Assert.True(maior > 0.999, $"o maior sorteio foi {maior}");
    }

    /// <summary>
    /// Os zeros do fim sao JOGADOS FORA, senao dois vetores diferentes
    /// representariam o mesmo polinomio e a igualdade pararia de significar o que
    /// deveria.
    /// </summary>
    [Fact]
    public void OsZerosDoFimSaoJogadosFora()
    {
        var comZeros = Polinomio.De(1, 2, 0, 0, 0);

        Assert.Equal(1, comZeros.Grau);
        Assert.Equal(Polinomio.De(1, 2), comZeros);
        Assert.Equal(2, comZeros.Termos);
    }

    [Fact]
    public void OPolinomioZeroTemGrauMenosUm()
    {
        Assert.Equal(-1, Polinomio.Zero.Grau);
        Assert.True(Polinomio.Zero.EhZero);
        Assert.Equal(Polinomio.Zero, Polinomio.De(0, 0, 0));
        Assert.Equal("0", Polinomio.Zero.ToString());
    }

    [Fact]
    public void ACesaoDeCoeficienteForaDaFaixaDaZero()
    {
        var p = Polinomio.De(1, 2, 3);

        Assert.Equal(3, p[2]);
        Assert.Equal(BigInteger.Zero, p[99]);
        Assert.Equal(BigInteger.Zero, p[-1]);
    }

    /// <summary>
    /// AVALIAR por Horner da a mesma coisa que calcular cada potencia, e gasta n
    /// multiplicacoes em vez de n(n+1)/2.
    /// </summary>
    [Fact]
    public void HornerDaOMesmoQueCalcularAsPotencias()
    {
        var sorteio = new Sorteio(5);

        for (var caso = 0; caso < 200; caso++)
        {
            var p = Polinomio.Sortear(sorteio.Entre(0, 12), sorteio, limite: 50);
            var x = (BigInteger)sorteio.Entre(-10, 10);

            var direto = BigInteger.Zero;
            var potencia = BigInteger.One;

            for (var i = 0; i < p.Termos; i++)
            {
                direto += p[i] * potencia;
                potencia *= x;
            }

            Assert.Equal(direto, p.Em(x));
        }
    }

    [Fact]
    public void ASomaEASubtracaoSaoOQueADistributivaDiz()
    {
        var sorteio = new Sorteio(9);

        for (var caso = 0; caso < 200; caso++)
        {
            var a = Polinomio.Sortear(sorteio.Entre(0, 10), sorteio, 100);
            var b = Polinomio.Sortear(sorteio.Entre(0, 10), sorteio, 100);
            var x = (BigInteger)sorteio.Entre(-5, 5);

            Assert.Equal(a.Em(x) + b.Em(x), (a + b).Em(x));
            Assert.Equal(a.Em(x) - b.Em(x), (a - b).Em(x));
        }
    }

    [Fact]
    public void SubtrairDeSiMesmoDaZero()
    {
        var sorteio = new Sorteio(2);

        for (var caso = 0; caso < 100; caso++)
        {
            var p = Polinomio.Sortear(sorteio.Entre(0, 20), sorteio);
            Assert.True((p - p).EhZero);
        }
    }

    /// <summary>
    /// DESLOCAR e multiplicar por x elevado a uma potencia: a avaliacao em x
    /// confere.
    /// </summary>
    [Fact]
    public void DeslocarEhMultiplicarPorUmaPotencia()
    {
        var sorteio = new Sorteio(4);

        for (var caso = 0; caso < 100; caso++)
        {
            var p = Polinomio.Sortear(sorteio.Entre(0, 10), sorteio, 50);
            var k = sorteio.Entre(0, 5);
            var x = (BigInteger)sorteio.Entre(2, 6);

            Assert.Equal(p.Em(x) * BigInteger.Pow(x, k), p.Deslocado(k).Em(x));
        }
    }

    /// <summary>
    /// Partir em duas metades e juntar de volta devolve o original, e e isso que
    /// torna o Karatsuba possivel.
    /// </summary>
    [Fact]
    public void PartirEmDuasMetadesEJuntarDevolveOOriginal()
    {
        var sorteio = new Sorteio(6);

        for (var caso = 0; caso < 200; caso++)
        {
            var p = Polinomio.Sortear(sorteio.Entre(0, 40), sorteio);
            var corte = sorteio.Entre(1, Math.Max(1, p.Termos));

            Assert.Equal(p, p.ParteBaixa(corte) + p.ParteAlta(corte).Deslocado(corte));
        }
    }

    /// <summary>
    /// O polinomio SORTEADO tem exatamente o grau pedido: um que as vezes tem grau
    /// menor estragaria qualquer medida que fale em grau.
    /// </summary>
    [Fact]
    public void OSorteadoTemExatamenteOGrauPedido()
    {
        var sorteio = new Sorteio(8);

        for (var grau = 0; grau <= 30; grau++)
            for (var caso = 0; caso < 20; caso++)
                Assert.Equal(grau, Polinomio.Sortear(grau, sorteio, 10).Grau);
    }

    [Fact]
    public void AvaliarModuloPrimoBateComAvaliarEDepoisReduzir()
    {
        var sorteio = new Sorteio(12);

        for (var caso = 0; caso < 200; caso++)
        {
            var p = Polinomio.Sortear(sorteio.Entre(0, 15), sorteio, 500);
            var x = sorteio.Entre(0, 1000);

            var exato = ((p.Em(x) % Ntt.Primo) + Ntt.Primo) % Ntt.Primo;

            Assert.Equal((long)exato, p.Em(x, Ntt.Primo));
        }
    }

    [Fact]
    public void ADigitacaoDoPolinomioEhLegivel()
    {
        Assert.Equal("3x^2 + -1x + 5", Polinomio.De(5, -1, 3).ToString());
        Assert.Equal("1", Polinomio.Um.ToString());
    }

    [Fact]
    public void ADigitosDoMaiorContaOTamanhoDoMaiorCoeficiente()
    {
        Assert.Equal(1, Polinomio.Zero.DigitosDoMaior());
        Assert.Equal(3, Polinomio.De(1, -100, 5).DigitosDoMaior());
        Assert.Equal(100, Polinomio.De(1).Vezes(BigInteger.Pow(10, 99)).DigitosDoMaior());
    }
}
