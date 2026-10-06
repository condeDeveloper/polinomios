using System.Numerics;
using Conde.Polinomios;
using Xunit;

namespace Conde.Polinomios.Testes;

public class FlutuanteTestes
{
    /// <summary>
    /// Com coeficientes pequenos, a transformada com virgula ACERTA, e e por isso
    /// que o defeito dela nao aparece em teste.
    /// </summary>
    [Fact]
    public void ComCoeficientesPequenosElaAcerta()
    {
        var sorteio = new Sorteio(3);

        for (var caso = 0; caso < 200; caso++)
        {
            var a = Polinomio.Sortear(sorteio.Entre(0, 60), sorteio, 100);
            var b = Polinomio.Sortear(sorteio.Entre(0, 60), sorteio, 100);

            Assert.Equal(Multiplicacao.Escolar(a, b).Produto, Flutuante.Multiplicar(a, b));
        }
    }

    /// <summary>
    /// O ACHADO: a partir de um certo tamanho ela ERRA, e nada na saida avisa.
    ///
    /// O vetor devolvido tem coeficientes inteiros, de tamanho plausivel, no grau
    /// certo. Simplesmente alguns deles nao sao os certos.
    /// </summary>
    [Fact]
    public void ComCoeficientesGrandesElaErraSemAvisar()
    {
        var sorteio = new Sorteio(3);

        var a = Polinomio.Sortear(511, sorteio, 10_000_000);
        var b = Polinomio.Sortear(511, sorteio, 10_000_000);

        var certo = Multiplicacao.Escolar(a, b).Produto;
        var comVirgula = Flutuante.Multiplicar(a, b);

        Assert.NotEqual(certo, comVirgula);

        // a saida continua com cara de resposta: mesmo grau, inteiros plausiveis
        Assert.Equal(certo.Grau, comVirgula.Grau);
        Assert.True(comVirgula.DigitosDoMaior() >= certo.DigitosDoMaior() - 1);
    }

    /// <summary>
    /// A TRANSFORMADA NUMERICA acerta exatamente onde a com virgula erra, porque
    /// ela nao arredonda nada.
    ///
    /// E ela tem o limite dela: o coeficiente precisa caber no primo. A diferenca e
    /// que o limite dela e VERIFICAVEL antes de rodar, e o da outra depende do
    /// acumulo do erro de arredondamento.
    /// </summary>
    [Fact]
    public void ATransformadaNumericaAcertaOndeAComVirgulaErra()
    {
        var sorteio = new Sorteio(3);

        var a = Polinomio.Sortear(255, sorteio, 1_000);
        var b = Polinomio.Sortear(255, sorteio, 1_000);

        Assert.True(Ntt.CabeNoPrimo(a, b));

        var certo = Multiplicacao.Escolar(a, b).Produto;

        Assert.Equal(certo, Ntt.Multiplicar(a, b));
    }

    /// <summary>
    /// O DESVIO ate o inteiro mais proximo avisa antes de o erro acontecer, e ele
    /// cresce com o tamanho dos coeficientes.
    /// </summary>
    [Fact]
    public void ODesvioCresceComOTamanhoDosCoeficientes()
    {
        var sorteio = new Sorteio(5);
        var anterior = -1.0;

        foreach (var limite in new[] { 100, 10_000, 1_000_000 })
        {
            var a = Polinomio.Sortear(255, sorteio, limite);
            var b = Polinomio.Sortear(255, sorteio, limite);

            var desvio = Flutuante.MaiorDesvio(a, b);

            Assert.True(desvio > anterior, $"com limite {limite} o desvio caiu para {desvio:E2}");
            anterior = desvio;
        }
    }

    /// <summary>
    /// O ACHADO QUE ME PEGOU: o aviso do desvio PARA DE FUNCIONAR exatamente
    /// quando o erro fica pior.
    ///
    /// Acima de dois elevado a cinquenta e tres, todo numero de precisao dupla ja E
    /// um inteiro: nao ha parte fracionaria nenhuma para medir. O desvio ate o
    /// inteiro mais proximo da ZERO, que e o valor de "arredondamento perfeito", e
    /// o produto esta completamente errado.
    ///
    /// Eu ia usar o desvio como a conferencia barata do metodo, e ele so avisa
    /// enquanto o erro ainda e pequeno.
    /// </summary>
    [Fact]
    public void OAvisoDoDesvioParaDeFuncionarQuandoOErroFicaPior()
    {
        var sorteio = new Sorteio(3);

        var a = Polinomio.Sortear(1023, sorteio, 1_000_000_000);
        var b = Polinomio.Sortear(1023, sorteio, 1_000_000_000);

        Assert.True(Flutuante.MaiorCoeficientePossivel(a, b) > Flutuante.LimiteDaPrecisaoDupla);
        Assert.Equal(0.0, Flutuante.MaiorDesvio(a, b), 12);
        Assert.NotEqual(Multiplicacao.Escolar(a, b).Produto, Flutuante.Multiplicar(a, b));
    }

    /// <summary>
    /// A conta do maior coeficiente possivel preve direito: enquanto ela fica bem
    /// abaixo do limite da precisao dupla, a transformada com virgula acerta.
    /// </summary>
    [Fact]
    public void AContaDoMaiorCoeficientePreveDireito()
    {
        var sorteio = new Sorteio(7);

        for (var caso = 0; caso < 60; caso++)
        {
            var grau = sorteio.Entre(10, 200);
            var limite = sorteio.Entre(10, 2000);

            var a = Polinomio.Sortear(grau, sorteio, limite);
            var b = Polinomio.Sortear(grau, sorteio, limite);

            if (Flutuante.MaiorCoeficientePossivel(a, b) * 1000 > Flutuante.LimiteDaPrecisaoDupla) continue;

            Assert.Equal(Multiplicacao.Escolar(a, b).Produto, Flutuante.Multiplicar(a, b));
        }
    }

    [Fact]
    public void ATransformadaComplexaEAInversaSeDesfazem()
    {
        var sorteio = new Sorteio(9);

        foreach (var tamanho in new[] { 2, 4, 8, 64, 256 })
        {
            var original = new System.Numerics.Complex[tamanho];
            for (var i = 0; i < tamanho; i++) original[i] = new System.Numerics.Complex(sorteio.Entre(-100, 100), 0);

            var copia = (System.Numerics.Complex[])original.Clone();

            Flutuante.Transformar(copia, inversa: false);
            Flutuante.Transformar(copia, inversa: true);

            for (var i = 0; i < tamanho; i++)
                Assert.True(Math.Abs(copia[i].Real - original[i].Real) < 1e-6);
        }
    }

    [Fact]
    public void MultiplicarPorZeroDaZero()
    {
        Assert.True(Flutuante.Multiplicar(Polinomio.De(1, 2, 3), Polinomio.Zero).EhZero);
        Assert.Equal(0.0, Flutuante.MaiorDesvio(Polinomio.De(1, 2), Polinomio.Zero), 12);
    }
}
