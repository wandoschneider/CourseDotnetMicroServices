## .NET Microservices <br>
Código criado a partir do treinamento da .Net Academy, objetivando a criação de um sistema completo de microserviços, com comunicação assíncrona e independente, bem como monitoria e observabilidade. A arquitetura de referência (após a conclusão de todos os módulos é:
![Alt text](https://user-images.githubusercontent.com/8496997/126161705-260b3b53-5fed-4e8d-86c1-710915c39d5b.png?raw=true ".Net Microservices")

## Tecnologias
Cada serviço é uma API REST .NET 5 com C#, banco de dados MongoDB, message broker RabbitMQ (utilizando o framework MassTransit para interação com as filas). A Autenticação implementa OpenID. A orquestração dos serviços é feita utilizando o Docker, através de docker-compose.

## Learning Path
![Alt text](https://dotnetmicroservices.com/images/LearningPath.png?raw=true "Learning Path")
