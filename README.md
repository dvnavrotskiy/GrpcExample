# gRPC via asp .netcore WebServices HowTo #

https://dotnetcorecentral.com/blog/grpc-in-asp-net-core/

У нас есть два веб-сервиса с REST-api, FirstService использует как клиент SecondService через сборку контрактов SecondService.Contacts.
Хотим добавить между ними взаимодействие gRPC, в качестве сервера останется SecondService, FirstService будет клиентом.
Главная идея в том, чтоб держать код логики максимально чистым от автогенерации. Желательно, чтоб клиентский сервис имел простой доступ к данным.

## 1. Серверная часть ##

1.1. Накидываем nuget-пакеты в сервера
- Grpc.Tools
- Grpc.Core
- Grpc.AspNetCore

1.2. В сборке контрактов SecondService.Contacts создаем папку Protos, в ней grpc-контракт Protos\SecondService.proto

	syntax = "proto3";

	option csharp_namespace = "SecondServiceApi";

	service SecondServiceApiProvider {
	  rpc GetBasicData (BasicRequest) returns (BasicResponse);
	}

	message BasicRequest{
	  string State = 1;
	}

	message BasicResponse{
	  string StatusString = 1;
	}
	
1.3. В файле проекта SecondService\SecondService.proj ссылаемся на контракт, указывая, что будем использовать его как сервер

	<ItemGroup>
        <Protobuf Include="..\SecondService.Contracts\Protos\SecondService.proto" GrpcServices="Server" />
    </ItemGroup>
	
1.4. Билдим SecondService (и далее после каждого изменения *.proto нужно ребилдить проект, чтоб IDE его понимало)

1.5. Описываем сам grpc-сервис Grpc\DataServiceGrpc.cs в SecondService, он выступит аналогом наследников BaseController для REST

	using Grpc.Core;
	using SecondService.Services;
	using SecondServiceApi;

	namespace SecondService.Grpc;

	public sealed class DataServiceGrpc : SecondServiceApiProvider.SecondServiceApiProviderBase
	{
		private readonly DataService dataService;

		public DataServiceGrpc(DataService dataService)
		{
			this.dataService = dataService;
		}
		
		public override Task<BasicResponse> GetBasicData(
			BasicRequest      request, 
			ServerCallContext serverCallContext)
		{
			var status = dataService.GetStatus(request.State);
			return Task.FromResult(new BasicResponse { StatusString = status });
		}
	}
	
Видно, что через DI вбрасывается DataService, метод GetBasicData получает из него данные, преобразуя контракты на входе и выходе

1.6. Меняем SecondService\Program.cs - добавляем grpc в сервисы `builder.Services.AddGrpc();

Там же включаем наш ендпоинт `app.MapGrpcService<DataServiceGrpc>();

1.7. Конфигурируем порты сервера, чтоб развести REST и gRPC, в SecondService\appsettings.json добавляем

	"Kestrel": {
		"Endpoints": {
		  "gRPC": {
			"Url": "http://localhost:5110",
			"Protocols": "Http2"
		  },
		  "Http": {
			"Url": "http://localhost:5100"
		  },
		  "Https": {
			"Url": "https://localhost:7091"
		  }
		}
	  }
	  
На этом можем считать, что с серверной частью закончено

## 2. Клиентская часть ##

2.1. Накидываем nuget-пакеты в сборку контрактов SecondService.Contacts - она выступит у нас базой для реализации клиента
- Grpc.Tools
- Grpc.Core
- Grpc.Net.Client
- Google.Protobuf

2.2. В сборке контрактов SecondService.Contacts уже есть grpc-контракт Protos\SecondService.proto - очень удобно,
в файле проекта SecondService.Contracts\SecondService.proj укажем, что будем использовать его как клиент

    <ItemGroup>
        <Protobuf Include="Protos\SecondService.proto" GrpcServices="Client" />
    </ItemGroup>
	
2.3. Билдим SecondService.Contacts

2.4. Реализуем сам класс клиента SecondService.Contacts\SecondServiceGrpcClient.cs

	using Grpc.Net.Client;
	using SecondServiceApi;

	namespace SecondService.Contracts;

	public sealed class SecondServiceGrpcClient
	{
		private readonly SecondServiceApiProvider.SecondServiceApiProviderClient client;

		public SecondServiceGrpcClient(string baseAddress)
		{
			var httpHandler = new HttpClientHandler
							  {
								  ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
							  };

			var channel = GrpcChannel.ForAddress(
				baseAddress,
				new GrpcChannelOptions {HttpHandler = httpHandler}
			);

			client = new SecondServiceApiProvider.SecondServiceApiProviderClient(channel);
		}

		public async Task<string> GetData(string state)
		{
			var result = await client.GetBasicDataAsync(new BasicRequest {State = state});
			return result.StatusString;
		}
	}
	
Метод GetData преобразует контракты на входе и выходе.
	
## 3. Использование в другом сервисе ##

3.1. В настройки сервиса-пользователя FirstService\appsettings.json добавляем адрес grpc-сервера

	"SecondServiceGrpc": "http://localhost:5110"
	
3.2. Добавляем проект SecondService.Contacts в зависимости сервиса FirstService

3.3. Получаем настройку из конфига прежде, чем закинуть в DI синглтон

	var secondSvcGrpcUri = cfg["SecondServiceGrpc"];
	if (string.IsNullOrEmpty(secondSvcGrpcUri))
		throw new Exception("appsettings.json must define SecondServiceGrpc as uri to SecondService GRPC API");
		
и добавляем в DI `.AddSingleton(new SecondServiceGrpcClient(secondSvcGrpcUri))
		
3.4. Далее используем как любой другой объект из DI: получаем через параметры конструктора и вызываем в коде

    [HttpGet("secondGrpc")]
    public async Task<ActionResult> GetSecondGrpc()
    {
        var status = await secondServiceGrpcClient.GetData("gRPC");
        
        logger.LogInformation("FirstService Home Controller GetSecondGrpc executed.");
        return Ok($"Second GRPC status: {status}");
    }
	
## Заключение ##

Теперь можно запустить оба сервиса и посмотреть их взаимодействие, например, используя Swagger на FirstService (https://localhost:7287/swagger/index.html).

Данное решение позволяет реализовать в одном сервисе и REST-api, и gRPC-api одновременно, при этом для сервера разделяет массив кода автогенерации,
использует общий файл контракта для сервера и клиента, максимально скрывает тонкости реализации для пользователя клиента.
