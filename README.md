
# Desafio Umbler

Esta é uma aplicação web que recebe um domínio e mostra suas informações de DNS.

Este é um exemplo real de sistema que utilizamos na Umbler.

Ex: Consultar os dados de registro do dominio `umbler.com`

**Retorno:**
- Name servers (ns254.umbler.com)
- IP do registro A (177.55.66.99)
- Empresa que está hospedado (Umbler)

Essas informações são descobertas através de consultas nos servidores DNS e de WHOIS.

*Obs: WHOIS (pronuncia-se "ruís") é um protocolo específico para consultar informações de contato e DNS de domínios na internet.*

Nesta aplicação, os dados obtidos são salvos em um banco de dados, evitando uma segunda consulta desnecessaria, caso seu TTL ainda não tenha expirado.

*Obs: O TTL é um valor em um registro DNS que determina o número de segundos antes que alterações subsequentes no registro sejam efetuadas. Ou seja, usamos este valor para determinar quando uma informação está velha e deve ser renovada.*

Tecnologias Backend utilizadas:

- C#
- Asp.Net Core
- MySQL
- Entity Framework

Tecnologias Frontend utilizadas:

- Webpack
- Babel
- ES7

Para rodar o projeto você vai precisar instalar:

- dotnet Core SDK (https://www.microsoft.com/net/download/windows dotnet Core 6.0.201 SDK)
- Um editor de código, acoselhamos o Visual Studio ou VisualStudio Code. (https://code.visualstudio.com/)
- NodeJs v17.6.0 para "buildar" o FrontEnd (https://nodejs.org/en/)
- Um banco de dados MySQL (vc pode rodar localmente ou criar um site PHP gratuitamente no app da Umbler https://app.umbler.com/ que lhe oferece o banco Mysql adicionamente)

Com as ferramentas devidamente instaladas, basta executar os seguintes comandos:

Para "buildar" o javascript basta executar:

`npm install`
`npm run build`

Para Rodar o projeto:

Execute a migration no banco mysql:

`dotnet tool update --global dotnet-ef`
`dotnet tool ef database update`

E após: 

`dotnet run` (ou clique em "play" no editor do vscode)

# Objetivos:

Se você rodar o projeto e testar um domínio, verá que ele já está funcionando. Porém, queremos melhorar varios pontos deste projeto:

# FrontEnd

 - Os dados retornados não estão formatados, e devem ser apresentados de uma forma legível.
 - Não há validação no frontend permitindo que seja submetido uma requsição inválida para o servidor (por exemplo, um domínio sem extensão).
 - Está sendo utilizado "vanilla-js" para fazer a requisição para o backend, apesar de já estar configurado o webpack. O ideal seria utilizar algum framework mais moderno como ReactJs ou Blazor.  

# BackEnd

 - Não há validação no backend permitindo que uma requisição inválida prossiga, o que ocasiona exceptions (erro 500).
 - A complexidade ciclomática do controller está muito alta, o ideal seria utilizar uma arquitetura em camadas.
 - O DomainController está retornando a própria entidade de domínio por JSON, o que faz com que propriedades como Id, Ttl e UpdatedAt sejam mandadas para o cliente web desnecessariamente. Retornar uma ViewModel (DTO) neste caso seria mais aconselhado.

# Testes

 - A cobertura de testes unitários está muito baixa, e o DomainController está impossível de ser testado pois não há como "mockar" a infraestrutura.
 - O Banco de dados já está sendo "mockado" graças ao InMemoryDataBase do EntityFramework, mas as consultas ao Whois e Dns não. 

# Dica

- Este teste não tem "pegadinha", é algo pensado para ser simples. Aconselhamos a ler o código, e inclusive algumas dicas textuais deixadas nos testes unitários. 
- Há um teste unitário que está comentado, que obrigatoriamente tem que passar.
- Diferencial: criar mais testes.

# Entrega

- Enviei o link do seu repositório com o código atualizado.
- O repositório deve estar público para que possamos acessar..
- Modifique Este readme adicionando informações sobre os motivos das mudanças realizadas.

# Modificações:

## BackEnd
- Inclui uma DTO para tratar a transferência de dados do objeto de domínio para o Front-End (DomainDTO.cs), sem expor dados sensíveis, como a Id, a fim de manter a proteção dos dados, entregando ao Front-End apenas o que será necessário para exibição: Nome, Ip e empresa que está hospedado.

- Inclui uma classe de validação utilizada pelo DomainController.cs para garantir que o nome do domínio recebido do Front-End é válido (DomainValidator.cs), segundo as regras de negócio do projeto, bloqueando domínios longos ou curtos demais, sem extensão, vazios e mal formatados.

- Reestruturei completamente o Back-End (Controllers, Services e Repositories), separando a lógica operacional em suas respectivas responsabilidades, assim, migrando para uma arquitetura em camadas e mais segura. Agora, o Controller é responsável apenas por tratar a requisição, o Service da lógica operacional, e o Repository das operações com banco de dados. Dessa forma, a estrutura do Back-End se torna mais limpa, sólida e escalável, já que se torna claro onde está presente cada funcionalidade e operação, de acordo com sua categoria.

## FrontEnd
- Migrei totalmente o Front-End para Blazor, mesmo com pouca experiência, consultei ferramentes de IA e documentação para implementar um cliente componentizado e com uma interface semelhante a identidade visual da Landing Page da própria Umbler, apenas por objetivo criativo. Essa migração foi feita com objetivo de possuir uma infraestrutura mais moderna no Front-End, retirando a fragilidade de utilizar HTML e JS puro. Por fim, a escolha de Blazor, ao invés de ReactJS, foi para manter a lógica operacional na mesma linguagem de código do Back-End e para experimentar e aprendar mais sobre a tecnologia da Blazor.

- Formatei os dados de retorno do Back-End para uma fornecer ao usuário uma exibição mais clara e legível, e inclui uma camada de validação antes de enviar o domínio para requisição, com uma estrutura parecida com a validação do próprio servidor Back-End, recusando domínios vazios, sem extensão, longos ou curtos demais, para assegurar que os dados recebidos no Back-End já estejam corretos e válidos.

- Removi completamente a estrutura de Front-End antes presente no projeto Desafio.Umbler (esse projeto tornou-se apenas de Back-End), já que a sua existência não era mais necessária, pois o cliente Front-End agora possui um projeto separado, Desafio.Umbler.Client, que foi criado com Blazor.

## Testes
- Reorganizei e reestruturei os testes completamente para se adequar a nova estrutura do Back-End separado pela responsabilidade de código, assim, os Testes agora são separados em Controllers, Services e Validators, cada um certifica que sua respectiva área do projeto está funcionando como esperado.

- Inclui testes novos para certificar o funcionamento de áreas novas do projeto, que surgiram conforme necessidade, como: Testes do serviço de domínio (DomainServiceTests.cs) e do validador de domínio (DomainValidatorTests.cs). Para o validador de domínio, inclui apenas testes unitários simples que certificam que os domínios inválidos são recusados e que os domínios válidos são aceitos apropriadamente. Agora, para os testes de serviço, inclui novos testes de integração, que se assemelham muito aos antigos testes que existiam no Controller, já que agora o núcleo da lógica operacional está presente no serviço, e não no controlador. Por fim, para os testes de Controller, inclui testes unitários e de integração, que certificam o funcionamento apropriado dos retornos para as requisições.
