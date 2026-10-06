# polinomios

Aritmética de polinômios do zero em C# e .NET 8: multiplicação escolar,
Karatsuba, transformada numérica módulo um primo, transformada com números
complexos, divisão com resto e máximo divisor comum. O juiz é um **teorema**: um
polinômio não nulo de grau d tem no máximo d raízes.

```
$ dotnet medidor.dll juiz

  grau   casos   escolar   karatsuba   transformada
     7     100       sim         sim            sim
    15     100       sim         sim            sim
    31     100       sim         sim            sim
    63     100       sim         sim            sim
```

Dele sai tudo: se dois polinômios de grau no máximo d concordam em **d+1 pontos
distintos**, eles são o mesmo polinômio. Não "parecidos": o mesmo. A diferença
entre eles tem grau no máximo d e tem d+1 raízes, então ela é o polinômio zero.

Comparar vetores de coeficientes diria que duas implementações fizeram a mesma
coisa. Avaliar nos pontos diz que as duas calcularam o **polinômio certo**.

## O ponto sorteado, e por que ele basta

```
  grau   faixa   tentativas   o primeiro ponto ja pegou   chance de falhar
    10      20       10,000                     49.69 %            50.00 %
    10     100       10,000                     90.33 %            10.00 %
    50     100       10,000                     49.98 %            50.00 %
    50  10,000       10,000                     99.70 %             0.50 %
```

A medida é feita no **pior caso possível**: a diferença entre os dois polinômios
é montada com o número máximo de raízes que o grau permite. E ela bate com a
conta: a chance de o ponto cair numa raiz é no máximo grau sobre faixa.

Com três pontos independentes, a chance cai ao cubo. Com grau mil e um milhão de
valores, é um em um bilhão, e é por isso que testar identidade de polinômio por
sorteio é um dos algoritmos probabilísticos mais usados que existem.

## Multiplicações de coeficiente, não segundos

```
$ dotnet medidor.dll custo

  grau        escolar     karatsuba   transformada   escolar/karatsuba
    15            256           256            272                 1.0x
    63          4,096         3,072          1,472                 1.3x
   255         65,536        27,648          7,424                 2.4x
  1023      1,048,576       248,832         35,840                 4.2x
```

Com grau 15, o Karatsuba gasta **exatamente o mesmo** que a escolar: o corte
impede a recursão, e ele *é* a escolar. Com grau 1023 ele gasta um quarto. E a
transformada gasta vinte e nove vezes menos que a escolar.

Karatsuba trocou quatro multiplicações de metade por **três**, e isso vira n
elevado a 1,585 em vez de n ao quadrado. Kolmogorov tinha conjecturado que n ao
quadrado era o mínimo; Karatsuba achou isto em uma semana, com vinte e três anos.

## A transformada que arredonda

```
$ dotnet medidor.dll virgula

  grau   coeficiente ate   maior coeficiente possivel   maior desvio   acertou
    63             1,000                     63170560      8.38E-009       sim
   255           100,000                2550764825088      1.14E-003       sim
   511        10,000,000            50970668051705856      5.00E-001       NAO
  1023     1,000,000,000       1022237562134509387776      0.00E+000       NAO
```

A transformada com números complexos é a famosa: cosseno, seno, raízes da unidade
no círculo. E ela multiplica polinômios de coeficientes inteiros devolvendo
números com vírgula que **precisam ser arredondados** no fim.

Enquanto os coeficientes são pequenos, o arredondamento acerta e ninguém percebe.
A precisão dupla guarda inteiros exatos até 2^53, que é nove mil trilhões, e o
maior coeficiente de um produto de grau 511 com coeficientes de dez milhões passa
de cinquenta mil trilhões.

A transformada **módulo um primo** não arredonda nada: a resposta é exata ou não
sai. O preço é escolher o primo certo, que precisa ter uma raiz da unidade da
ordem certa, e é por isso que primos como 2^23 × 119 + 1 aparecem em todo código
de competição.

## O máximo divisor comum e os números que explodem

```
$ dotnet medidor.dll maximo

  grau   fator   passos   maior coeficiente (digitos)   com o conteudo tirado
     6       2        5                          103                      14
     8       3        6                          247                      15
    10       3        8                         1429                      25
    12       4        9                         2616                      29
```

Polinômios de entrada com coeficientes de **três dígitos**. O algoritmo de
Euclides direto, com pseudodivisão, passa por coeficientes de **2.616 dígitos**
em nove passos. A resposta final é minúscula; o caminho até ela, não.

Esse é um dos exemplos mais claros de uma coisa que a notação assintótica não vê:
o número de operações é pequeno, e cada operação fica cara.

O conserto é uma linha a mais no laço: dividir cada resto pelo **conteúdo**, que é
o máximo divisor comum dos coeficientes. Isso não muda o polinômio a menos de uma
constante, que é tudo que importa num máximo divisor comum, e leva 2.616 dígitos
para 29.

## O que as medidas me corrigiram

**A transformada módulo um primo devolve os coeficientes sem sinal, e eu li a
saída como se eles tivessem.** Um coeficiente de -600 sai como o primo menos 600,
que é um número de nove dígitos com cara de resposta. A conta modular estava
certa; a leitura dela, não. Os coeficientes de entrada têm sinal e o resto da
divisão não tem, e o conserto é devolver tudo na faixa simétrica em torno do zero.

**O aviso do arredondamento para de funcionar exatamente quando o erro fica
pior.** Eu ia usar "a distância até o inteiro mais próximo" como a conferência
barata da transformada com vírgula, e ela funciona na faixa do meio: 8e-9, depois
1e-3, depois 0,5 e o resultado errado. Acima de 2^53, todo número de precisão
dupla **já é** um inteiro, não há parte fracionária nenhuma para medir, e o desvio
dá **zero**, que é o valor de "arredondamento perfeito". Na última linha da tabela
o desvio é zero e o produto está completamente errado.

**A conta fechada do Karatsuba é um teto, e eu tinha escrito que ela bate.** A
contagem medida com grau 1023 dá 248.800 e a conta dá 248.832. A diferença é
pequena e tem motivo: somar as duas metades às vezes **cancela** o coeficiente de
maior grau, o polinômio fica com um termo a menos e aquele ramo da recursão gasta
um pouco menos. A conta fechada não sabe disso porque ela só olha tamanhos.

**Contar multiplicações sempre prefere o corte menor, e o corte certo não é
esse.** Com grau 1023, o Karatsuba com corte 1 gasta 59.018 multiplicações e com
corte 32 gasta 248.832, quatro vezes mais. E o corte de verdade de qualquer
implementação fica em algumas dezenas, porque o que a contagem de multiplicações
**não** conta (as somas, as cópias dos vetores, as chamadas recursivas) é o que
decide. É o limite honesto da medida que o repositório inteiro usa.

**O teste do ponto sorteado precisa do pior caso para medir o que diz medir.** A
primeira versão usava uma diferença igual a x elevado a k, que tem uma raiz só, e
o primeiro ponto pegava a diferença em 100% das tentativas, em todas as faixas. O
número ficava lindo e não dizia nada sobre o teorema, que fala do **pior** caso. A
versão de agora monta a diferença com o número máximo de raízes que o grau
permite, e aí os 49,69% batem com os 50% da conta.

## Por que o juiz é o teorema

O teorema das raízes é o único juiz deste repositório que não precisa de uma
segunda implementação. Ele é uma afirmação sobre **todos** os polinômios, e ele
transforma "as duas implementações concordaram" em "as duas calcularam a mesma
função".

E ele também é testado: um polinômio montado com d raízes conhecidas tem
exatamente d, conferido por força bruta numa faixa larga de inteiros; polinômios
sorteados nunca têm mais raízes que o grau; e concordar em menos pontos que o grau
comprovadamente não basta.

Os coeficientes são inteiros grandes e exatos. Multiplicar dois polinômios de grau
mil com coeficientes de dez dígitos dá coeficientes de vinte e três dígitos, e a
precisão dupla guarda dezesseis: o resultado não é "quase certo", é outro
polinômio.

O sorteio é de um gerador congruente escrito aqui, e não do sorteio da
biblioteca, cuja documentação não promete a mesma sequência entre versões nem
entre sistemas. A integração contínua roda em três.

## As peças

| arquivo | o que faz |
| --- | --- |
| `Polinomio.cs` | coeficientes inteiros exatos, Horner e o sorteio |
| `Multiplicacao.cs` | a escolar e o Karatsuba, contando multiplicações |
| `Ntt.cs` | a transformada módulo um primo, sem arredondar nada |
| `Identidade.cs` | o teorema das raízes e o teste por avaliação: o juiz |
| `Divisao.cs` | pseudodivisão, máximo divisor comum e o conteúdo |
| `Flutuante.cs` | a transformada com números complexos, e onde ela erra |
| `Medidas.cs` | as tabelas que o medidor imprime e que os testes conferem |

## Como rodar

```
dotnet test testes/Polinomios.Testes/Polinomios.Testes.csproj -c Release
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

As medidas aceitam `juiz`, `custo`, `virgula`, `maximo` e `tudo`.

## Licença

MIT.
