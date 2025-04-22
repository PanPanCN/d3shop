﻿using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NetCorePal.D3Shop.Domain.AggregatesModel.Identity.ClientUserAggregate;
using NetCorePal.D3Shop.Infrastructure;
using NetCorePal.D3Shop.Web.Controllers.Identity.Client.Requests;
using NetCorePal.D3Shop.Web.Controllers.Identity.Client.Responses;
using NetCorePal.Extensions.Dto;

namespace NetCorePal.D3Shop.Web.Tests.Identity;

[Collection("web")]
public class ClientUserAccountControllerIntegrationTests
{
    private readonly HttpClient _client;
    private readonly MyWebApplicationFactory _factory;

    public ClientUserAccountControllerIntegrationTests(
        MyWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.WithWebHostBuilder(builder => { builder.ConfigureServices(_ => { }); })
            .CreateClient();
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsJwtToken()
    {
        // Arrange
        var request = new ClientUserRegisterRequest
        (
            "test_user",
            "avatar.png",
            "13800138000",
            "Test@123456",
            "test@example.com"
        );

        // Act
        var response = await _client.PostAsNewtonsoftJsonAsync(
            "/api/ClientUserAccount/register", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<ClientUserId>>();
        Assert.NotNull(result?.Data);
        Assert.True(result.Data.Id > 0);

        // 验证数据库是否创建了用户
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.ClientUsers
            .FirstOrDefaultAsync(u => u.Phone == request.Phone);
        Assert.NotNull(user);
        Assert.Equal(request.NickName, user.NickName);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtToken()
    {
        // 先注册用户
        var registerRequest = new ClientUserRegisterRequest
        (
            "login_test",
            "avatar.png",
            "13800138001",
            "Test@123456",
            "login@test.com"
        );
        await _client.PostAsJsonAsync("/api/ClientUserAccount/register", registerRequest);

        // 登录请求
        var loginRequest = new ClientUserLoginRequest
        (
            "13800138001",
            "Test@123456",
            "1"
        );

        // Act
        var response = await _client.PostAsNewtonsoftJsonAsync(
            "/api/ClientUserAccount/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResponseData<ClientUserLoginResponse>>();
        var token = result?.Data;
        Assert.NotNull(token?.Token);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // 注册用户
        var registerRequest = new ClientUserRegisterRequest
        (
            "nick",
            "avatar.png",
            "13800138002",
            "CorrectPassword",
            "invalid_test"
        );
        await _client.PostAsJsonAsync("/api/ClientUserAccount/register", registerRequest);

        // 使用错误密码登录
        var loginRequest = new ClientUserLoginRequest
        (
            "13800138002",
            "WrongPassword",
            "1"
        );

        // Act & Assert
        var response = await _client.PostAsJsonAsync(
            "/api/ClientUserAccount/login", loginRequest);
        var result = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<ClientUserLoginResponse>>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("用户名或密码错误", result.Message);
    }


    [Fact]
    public async Task GetRefreshToken_ValidRequest_ReturnsRefreshToken()
    {
        // 先注册用户
        var registerRequest = new ClientUserRegisterRequest
        (
            "GetRefreshToken_test",
            "avatar.png",
            "13800138003",
            "Test@123456",
            "login@test.com"
        );
        await _client.PostAsJsonAsync("/api/ClientUserAccount/register", registerRequest);

        // 登录请求
        var loginRequest = new ClientUserLoginRequest
        (
            "13800138003",
            "Test@123456",
            "1"
        );

        // Act
        var response = await _client.PostAsNewtonsoftJsonAsync(
            "/api/ClientUserAccount/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResponseData<ClientUserLoginResponse>>();
        var token = result?.Data;
        Assert.NotNull(token?.Token);

        var getRefreshTokenResponse = await _client.PutAsNewtonsoftJsonAsync(
            "/api/ClientUserAccount/getRefreshToken", token.RefreshToken);


        // Assert
        response.EnsureSuccessStatusCode();
        var getRefreshTokenResult = await getRefreshTokenResponse.Content
            .ReadFromNewtonsoftJsonAsync<ResponseData<ClientUserGetRefreshTokenResponse>>();
        var refreshToken = getRefreshTokenResult?.Data;
        Assert.NotNull(refreshToken?.Token);
        Assert.NotNull(refreshToken.RefreshToken);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.ClientUsers.Include(clientUser => clientUser.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Phone == registerRequest.Phone);
        Assert.NotNull(user);
        Assert.Equal(user.RefreshTokens.First().Token, refreshToken.RefreshToken);
    }
}