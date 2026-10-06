using System.Numerics;
using Conde.Polinomios;
using Xunit;

namespace Conde.Polinomios.Testes;

public class DivisaoTestes
{
    /// <summary>
    /// A IDENTIDADE que a pseudodivisao promete: lider elevado a k vezes o
    /// dividendo e quociente vezes divisor mais resto.
    ///
    /// E ela que torna o resultado util, e e ela que o teste confere, em vez de
    /// confiar nos coeficientes.
    /// </summary>
    [Fact]
    public void AIdentidadeDaPseudodivisaoVale()
    {
        var sorteio = new Sorteio(3);

        for (var caso = 0; caso < 300; caso++)
        {
            var dividendo = Polinomio.Sortear(sorteio.Entre(0, 12), sorteio, 50);
            var divisor = Polinomio.Sortear(sorteio.Entre(1, 8), sorteio, 50);

            var (quociente, resto) = Divisao.Pseudodividir(dividendo, divisor);
            var esquerda = Divisao.DividendoAjustado(dividendo, divisor);
            var direita = Multiplicacao.Escolar(quociente, divisor).Produto + resto;

            Assert.True(Identidade.SaoIguais(esquerda, direita), $"caso {caso}");
        }
    }

    /// <summary>
    /// O RESTO tem grau menor que o divisor, que e o que "resto" quer dizer.
    /// </summary>
    [Fact]
    public void ORestoTemGrauMenorQueODivisor()
    {
        var sorteio = new Sorteio(5);

        for (var caso = 0; caso < 300; caso++)
        {
            var dividendo = Polinomio.Sortear(sorteio.Entre(0, 15), sorteio, 50);
            var divisor = Polinomio.Sortear(sorteio.Entre(1, 8), sorteio, 50);

            var resto = Divisao.Pseudodividir(dividendo, divisor).Resto;

            Assert.True(resto.Grau < divisor.Grau, $"resto de grau {resto.Grau} para divisor de grau {divisor.Grau}");
        }
    }

    [Fact]
    public void DividirPorUmDivisorDeGrauMaiorDevolveTudoNoResto()
    {
        var dividendo = Polinomio.De(1, 2);
        var divisor = Polinomio.De(1, 1, 1);

        var (quociente, resto) = Divisao.Pseudodividir(dividendo, divisor);

        Assert.True(quociente.EhZero);
        Assert.Equal(dividendo, resto);
    }

    [Fact]
    public void DividirPorZeroEhRecusado()
    {
        Assert.Throws<DivideByZeroException>(() => Divisao.Pseudodividir(Polinomio.De(1, 2), Polinomio.Zero));
    }

    /// <summary>
    /// Quando a divisao e EXATA, o resto e zero e o quociente e o outro fator, a
    /// menos do ajuste.
    /// </summary>
    [Fact]
    public void QuandoADivisaoEhExataORestoEhZero()
    {
        var sorteio = new Sorteio(7);

        for (var caso = 0; caso < 200; caso++)
        {
            var fator = Polinomio.Sortear(sorteio.Entre(1, 5), sorteio, 20);
            var outro = Polinomio.Sortear(sorteio.Entre(1, 5), sorteio, 20);
            var produto = Multiplicacao.Escolar(fator, outro).Produto;

            Assert.True(Divisao.Pseudodividir(produto, fator).Resto.EhZero, $"caso {caso}");
        }
    }

    /// <summary>
    /// O CONTEUDO e o maximo divisor comum dos coeficientes, e tirar ele nao muda
    /// as raizes do polinomio.
    /// </summary>
    [Fact]
    public void TirarOConteudoNaoMudaAsRaizes()
    {
        var sorteio = new Sorteio(9);

        for (var caso = 0; caso < 200; caso++)
        {
            var p = Polinomio.Sortear(sorteio.Entre(1, 10), sorteio, 100);
            var multiplicado = p.Vezes(sorteio.Entre(2, 50));

            var sem = Divisao.SemConteudo(multiplicado);

            Assert.Equal(BigInteger.One, Divisao.Conteudo(sem));
            Assert.Equal(multiplicado.Grau, sem.Grau);

            // as raizes sao as mesmas: um e multiplo do outro
            for (var x = -20; x <= 20; x++)
                Assert.Equal(multiplicado.Em(x).IsZero, sem.Em(x).IsZero);
        }
    }

    /// <summary>
    /// O MAXIMO DIVISOR COMUM acha mesmo um divisor comum: ele divide os dois sem
    /// deixar resto.
    /// </summary>
    [Fact]
    public void OMaximoDivideOsDoisSemDeixarResto()
    {
        var sorteio = new Sorteio(11);

        for (var caso = 0; caso < 60; caso++)
        {
            var (a, b, _) = Divisao.ComFatorComum(sorteio.Entre(6, 10), sorteio.Entre(2, 4), sorteio, 50);
            var maximo = Divisao.MaximoPrimitivo(a, b).Divisor;

            Assert.False(maximo.EhZero);
            Assert.True(Divisao.Pseudodividir(a, maximo).Resto.EhZero, $"nao divide o primeiro, caso {caso}");
            Assert.True(Divisao.Pseudodividir(b, maximo).Resto.EhZero, $"nao divide o segundo, caso {caso}");
        }
    }

    /// <summary>
    /// E o grau dele e pelo menos o do fator comum que foi plantado: ele acha pelo
    /// menos o que esta la.
    /// </summary>
    [Fact]
    public void OMaximoAchaPeloMenosOFatorPlantado()
    {
        var sorteio = new Sorteio(13);

        for (var caso = 0; caso < 60; caso++)
        {
            var grauDoFator = sorteio.Entre(2, 4);
            var (a, b, fator) = Divisao.ComFatorComum(sorteio.Entre(7, 11), grauDoFator, sorteio, 50);

            var maximo = Divisao.MaximoPrimitivo(a, b).Divisor;

            Assert.True(maximo.Grau >= fator.Grau, $"achou grau {maximo.Grau}, o fator tinha {fator.Grau}");
            Assert.True(Divisao.Pseudodividir(maximo, Divisao.SemConteudo(fator)).Resto.EhZero);
        }
    }

    /// <summary>
    /// O ACHADO: os coeficientes EXPLODEM no caminho.
    ///
    /// Com polinomios de entrada de tres digitos e grau doze, o maximo divisor
    /// comum pelo Euclides direto passa por coeficientes de milhares de digitos. A
    /// resposta final e minuscula; o caminho ate ela, nao.
    ///
    /// Esse e um dos exemplos mais claros de uma coisa que a notacao assintotica
    /// nao ve: o numero de operacoes e pequeno, e cada operacao fica cara.
    /// </summary>
    [Fact]
    public void OsCoeficientesExplodemNoCaminho()
    {
        var sorteio = new Sorteio(5);
        var (a, b, _) = Divisao.ComFatorComum(12, 4, sorteio);

        var ingenuo = Divisao.MaximoIngenuo(a, b);

        Assert.True(a.DigitosDoMaior() <= 6, $"a entrada ja tinha {a.DigitosDoMaior()} digitos");
        Assert.True(ingenuo.MaiorDigitos > 500, $"o caminho so chegou a {ingenuo.MaiorDigitos} digitos");
        Assert.True(ingenuo.Passos < 20, $"foram {ingenuo.Passos} passos");
    }

    /// <summary>
    /// E UMA LINHA A MAIS resolve: dividir cada resto pelo conteudo dele.
    ///
    /// Os dois acham o mesmo divisor a menos de uma constante, e e por isso que
    /// tirar o conteudo e de graca em termos de resposta.
    /// </summary>
    [Fact]
    public void TirarOConteudoDeCadaRestoResolve()
    {
        var sorteio = new Sorteio(5);

        foreach (var (grau, fator) in new[] { (6, 2), (8, 3), (10, 3), (12, 4) })
        {
            var (a, b, _) = Divisao.ComFatorComum(grau, fator, sorteio);

            var ingenuo = Divisao.MaximoIngenuo(a, b);
            var primitivo = Divisao.MaximoPrimitivo(a, b);

            Assert.True(primitivo.MaiorDigitos * 5 < ingenuo.MaiorDigitos,
                $"grau {grau}: ingenuo {ingenuo.MaiorDigitos} digitos, primitivo {primitivo.MaiorDigitos}");

            // os dois acham o MESMO divisor, a menos de uma constante
            Assert.Equal(Divisao.SemConteudo(ingenuo.Divisor), Divisao.SemConteudo(primitivo.Divisor));
        }
    }
}
