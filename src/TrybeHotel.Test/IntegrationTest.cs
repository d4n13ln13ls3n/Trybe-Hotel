namespace TrybeHotel.Test;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using TrybeHotel.Models;
using TrybeHotel.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Text;
using TrybeHotel.Dto;

public class IntegrationTest: IClassFixture<WebApplicationFactory<Program>>
{
     public HttpClient _clientTest;

     public IntegrationTest(WebApplicationFactory<Program> factory)
    {
        //_factory = factory;
        _clientTest = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TrybeHotelContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ContextTest>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTestDatabase");
                });
                services.AddScoped<ITrybeHotelContext, ContextTest>();
                services.AddScoped<ICityRepository, CityRepository>();
                services.AddScoped<IHotelRepository, HotelRepository>();
                services.AddScoped<IRoomRepository, RoomRepository>();
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                using (var appContext = scope.ServiceProvider.GetRequiredService<ContextTest>())
                {
                    appContext.Database.EnsureCreated();
                    appContext.Database.EnsureDeleted();
                    appContext.Database.EnsureCreated();
                    appContext.Cities.Add(new City {CityId = 1, Name = "Manaus"});
                    appContext.Cities.Add(new City {CityId = 2, Name = "Palmas"});
                    appContext.SaveChanges();
                    appContext.Hotels.Add(new Hotel {HotelId = 1, Name = "Trybe Hotel Manaus", Address = "Address 1", CityId = 1});
                    appContext.Hotels.Add(new Hotel {HotelId = 2, Name = "Trybe Hotel Palmas", Address = "Address 2", CityId = 2});
                    appContext.Hotels.Add(new Hotel {HotelId = 3, Name = "Trybe Hotel Ponta Negra", Address = "Addres 3", CityId = 1});
                    appContext.SaveChanges();
                    appContext.Rooms.Add(new Room { RoomId = 1, Name = "Room 1", Capacity = 2, Image = "Image 1", HotelId = 1 });
                    appContext.Rooms.Add(new Room { RoomId = 2, Name = "Room 2", Capacity = 3, Image = "Image 2", HotelId = 1 });
                    appContext.Rooms.Add(new Room { RoomId = 3, Name = "Room 3", Capacity = 4, Image = "Image 3", HotelId = 1 });
                    appContext.Rooms.Add(new Room { RoomId = 4, Name = "Room 4", Capacity = 2, Image = "Image 4", HotelId = 2 });
                    appContext.Rooms.Add(new Room { RoomId = 5, Name = "Room 5", Capacity = 3, Image = "Image 5", HotelId = 2 });
                    appContext.Rooms.Add(new Room { RoomId = 6, Name = "Room 6", Capacity = 4, Image = "Image 6", HotelId = 2 });
                    appContext.Rooms.Add(new Room { RoomId = 7, Name = "Room 7", Capacity = 2, Image = "Image 7", HotelId = 3 });
                    appContext.Rooms.Add(new Room { RoomId = 8, Name = "Room 8", Capacity = 3, Image = "Image 8", HotelId = 3 });
                    appContext.Rooms.Add(new Room { RoomId = 9, Name = "Room 9", Capacity = 4, Image = "Image 9", HotelId = 3 });
                    appContext.SaveChanges();
                }
            });
        }).CreateClient();
    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "GET /city deve retornar 200 OK")]
    [InlineData("/city")]
    public async Task TestGetCities(string url)
    {
        var response = await _clientTest.GetAsync(url);
        Assert.Equal(System.Net.HttpStatusCode.OK, response?.StatusCode);
    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "GET /hotel deve retornar 200 OK")]
    [InlineData("/hotel")]
    public async Task TestGetHotels(string url)
    {
        var response = await _clientTest.GetAsync(url);
        Assert.Equal(System.Net.HttpStatusCode.OK, response?.StatusCode);
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "POST /city deve retornar 201 Created")]
    public async Task TestPostCity()
    {
        var newCity = new { Name = "Belém" };
        var content = new StringContent(
            JsonConvert.SerializeObject(newCity),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _clientTest.PostAsync("/city", content);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "POST /hotel deve retornar 201 Created")]
    public async Task TestPostHotel()
    {
        var newHotel = new { Name = "Hotel Teste", Address = "Endereço Teste", CityId = 1 };
        var content = new StringContent(
            JsonConvert.SerializeObject(newHotel),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _clientTest.PostAsync("/hotel", content);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "POST /room deve retornar 201 Created")]
    public async Task TestPostRoom()
    {
        var newRoom = new { Name = "Quarto Teste", Capacity = 2, Image = "Imagem", HotelId = 1 };
        var content = new StringContent(
            JsonConvert.SerializeObject(newRoom),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _clientTest.PostAsync("/room", content);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "DELETE /room/{roomId} deve retornar 204 quando o quarto existe")]
    public async Task TestDeleteExistingRoom()
    {
        // Cria um quarto só para este teste
        var roomToCreate = new Room
        {
            Name = "Quarto para deletar",
            Capacity = 1,
            Image = "Imagem",
            HotelId = 1
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(roomToCreate),
            Encoding.UTF8,
            "application/json"
        );

        var postResponse = await _clientTest.PostAsync("/room", content);
        postResponse.EnsureSuccessStatusCode();

        var body = await postResponse.Content.ReadAsStringAsync();
        var createdRoom = JsonConvert.DeserializeObject<RoomDto>(body);

        var deleteResponse = await _clientTest.DeleteAsync($"/room/{createdRoom!.RoomId}");

        Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "DELETE /room/{roomId} deve retornar 404 quando o quarto não existe")]
    public async Task TestDeleteNonExistingRoom()
    {
        var response = await _clientTest.DeleteAsync("/room/9999");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "GET /city deve retornar as cidades cadastradas corretamente")]
    public async Task TestGetCitiesContent()
    {
        var response = await _clientTest.GetAsync("/city");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        var cities = JsonConvert.DeserializeObject<List<CityDto>>(body) ?? new List<CityDto>();

        // Como outros testes fazem POST /city, usamos >= 2 em vez de == 2
        Assert.True(cities.Count >= 2);
        Assert.Contains(cities, c => c.Name == "Manaus");
        Assert.Contains(cities, c => c.Name == "Palmas");
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "POST /city deve persistir a cidade que aparece depois no GET /city")]
    public async Task TestPostCityThenGet()
    {
        var newCity = new { Name = "Belém" };
        var content = new StringContent(
            JsonConvert.SerializeObject(newCity),
            Encoding.UTF8,
            "application/json"
        );

        var postResponse = await _clientTest.PostAsync("/city", content);
        Assert.Equal(System.Net.HttpStatusCode.Created, postResponse.StatusCode);

        var getResponse = await _clientTest.GetAsync("/city");
        getResponse.EnsureSuccessStatusCode();

        var body = await getResponse.Content.ReadAsStringAsync();
        var cities = JsonConvert.DeserializeObject<List<CityDto>>(body) ?? new List<CityDto>();

        Assert.Contains(cities, c => c.Name == "Belém");
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "GET /hotel deve retornar todos os hotéis cadastrados")]
    public async Task TestGetHotelsContent()
    {
        var response = await _clientTest.GetAsync("/hotel");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        var hotels = JsonConvert.DeserializeObject<List<HotelDto>>(body) ?? new List<HotelDto>();

        // Como outros testes fazem POST /hotel, usamos >= 3
        Assert.True(hotels.Count >= 3);
        Assert.Contains(hotels, h => h.Name == "Trybe Hotel Manaus");
        Assert.Contains(hotels, h => h.Name == "Trybe Hotel Palmas");
        Assert.Contains(hotels, h => h.Name == "Trybe Hotel Ponta Negra");
    }

    [Trait("Category", "Meus testes")]
    [Theory(DisplayName = "GET /room/{hotelId} deve retornar apenas quartos do hotel informado")]
    [InlineData(1, 3)]
    [InlineData(2, 3)]
    [InlineData(3, 3)]
    public async Task TestGetRoomsByHotelId(int hotelId, int expectedCount)
    {
        var response = await _clientTest.GetAsync($"/room/{hotelId}");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        var rooms = JsonConvert.DeserializeObject<List<dynamic>>(body)!;

        Assert.Equal(expectedCount, rooms.Count);
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "GET /room/{hotelId} deve retornar lista vazia para hotel sem quartos")]
    public async Task TestGetRoomsForHotelWithoutRooms()
    {
        // HotelId 999 não existe no banco de teste
        var response = await _clientTest.GetAsync("/room/999");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        var rooms = JsonConvert.DeserializeObject<List<dynamic>>(body)!;

        Assert.Empty(rooms);
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "POST /room deve persistir o quarto que aparece no GET /room/{hotelId}")]
    public async Task TestPostRoomThenGet()
    {
        var newRoom = new { Name = "Quarto Integração", Capacity = 2, Image = "Imagem", HotelId = 1 };
        var content = new StringContent(
            JsonConvert.SerializeObject(newRoom),
            Encoding.UTF8,
            "application/json"
        );

        var postResponse = await _clientTest.PostAsync("/room", content);
        postResponse.EnsureSuccessStatusCode();

        var getResponse = await _clientTest.GetAsync("/room/1");
        getResponse.EnsureSuccessStatusCode();

        var body = await getResponse.Content.ReadAsStringAsync();
        var rooms = JsonConvert.DeserializeObject<List<RoomDto>>(body) ?? new List<RoomDto>();

        Assert.Contains(rooms, r => r.Name == "Quarto Integração");
    }

    [Trait("Category", "Meus testes")]
    [Fact(DisplayName = "DELETE /room deve ser idempotente: primeiro 204, depois 404")]
    public async Task TestDeleteRoomTwice()
    {
        // 1. Cria um quarto para deletar
        var roomToCreate = new { Name = "Quarto para deletar 2x", Capacity = 1, Image = "Img", HotelId = 1 };
        var content = new StringContent(
            JsonConvert.SerializeObject(roomToCreate),
            Encoding.UTF8,
            "application/json"
        );

        var postResponse = await _clientTest.PostAsync("/room", content);
        postResponse.EnsureSuccessStatusCode();

        var body = await postResponse.Content.ReadAsStringAsync();
        var createdRoom = JsonConvert.DeserializeObject<RoomDto>(body);
        Assert.NotNull(createdRoom);

        int roomId = createdRoom!.RoomId;

        // 2. Primeiro DELETE → 204
        var delete1 = await _clientTest.DeleteAsync($"/room/{roomId}");
        Assert.Equal(System.Net.HttpStatusCode.NoContent, delete1.StatusCode);

        // 3. Segundo DELETE → 404
        var delete2 = await _clientTest.DeleteAsync($"/room/{roomId}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, delete2.StatusCode);
    }
}