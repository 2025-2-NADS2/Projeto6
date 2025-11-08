using EntregaEstruturaDados;

// Atividades:
Atividades Comunidade = new Atividades(1,"Comunidade", "Atividades relacionadas a comunidade", "Simulação Imagem");
Atividades Parturientes = new Atividades(2, "Parturientes", "Atividades relacionadas as gravidas e parturientes", "Simulação Imagem2");

//Transparência: com intuíto de fácil busca na hora do download dos pdf's em banco de dados, foi criado a variavel 'nome', que sera oculta ao usuario
//mas internamente salva o nome do pdf de forma padrão aceita no banco de dados
Transparência Doc2 = new Transparência("Levantamento de gastos", 2);
Transparência Doc1 = new Transparência("Fluxo de caixa das Doações", 1);

// Teste do algoritmo Bubble Sort
// - Foram criadas subclasses de TRANSPARENCIA e ATIVIDADES apenas para simulação.
// - Cada instância armazena dados em listas internas:
// COMUNIDADE → guarda objetos ArquivoPDF (via addPdf)
// PARTURIENTES → guarda objetos tipoAtv (via addTipoAtv)

Doc2.addPdf(2, "Gasto alimenticio mensal.pdf");
Comunidade.addTipoatv(3, "Sopa Comunitaria");
Doc2.addPdf(1, "Gasto reforma anual.pdf");
Parturientes.addTipoatv(2, "apoio ao Pré-natal");
Doc1.addPdf(1, "Informe dos doadores.pdf");
Comunidade.addTipoatv(1, "Entrega de cesta básica");
Parturientes.addTipoatv(3, "Apoio pós-operatorio");
Doc1.addPdf(3, "Doações via parceria GOV.pdf");
Comunidade.addTipoatv(2, "Incentivo a vacinação");
Doc1.addPdf(2, "Rendimento patrocinadores.pdf");
Parturientes.addTipoatv(1, "Doação de fraldas");
Doc2.addPdf(3, "Gasto da sopa comunitária.pdf");
// gasto da sopa comunitaria.pdf e demais com esta nomenclatura para simbolizar um arquivo pdf


// Dados desordenados:
Console.WriteLine("|*|----------------||DESORDENADO||------------------|*|");
Console.WriteLine("ATIVIDADES:");
Comunidade.descrever();
Parturientes.descrever();
Console.WriteLine();
Console.WriteLine("TRANSPARÊNCIA");
Doc1.descrever();
Doc2.descrever();
Console.WriteLine("|*|--------------------------------------------|*|");
Console.WriteLine();

//Ordenando os Objetos pelo ID
Comunidade.bubbleSort();
Parturientes.bubbleSort();
Doc1.bubbleSort();
Doc2.bubbleSort();

//Resultado da ordenação:
Console.WriteLine("|*|------------------||ORDENADO||------------------|*|");
Console.WriteLine("ATIVIDADES:");
Comunidade.descrever();
Parturientes.descrever();
Console.WriteLine();
Console.WriteLine("TRANSPARÊNCIA");
Doc1.descrever();
Doc2.descrever();
Console.WriteLine("|*|--------------------------------------------|*|");









