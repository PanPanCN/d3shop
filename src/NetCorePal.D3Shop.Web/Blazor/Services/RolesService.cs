﻿using NetCorePal.D3Shop.Admin.Shared.Requests;
using NetCorePal.D3Shop.Admin.Shared.Responses;
using NetCorePal.D3Shop.Domain.AggregatesModel.Identity.RoleAggregate;
using NetCorePal.D3Shop.Web.Admin.Client.Services;
using NetCorePal.D3Shop.Web.Controllers.Identity;
using NetCorePal.Extensions.Dto;

namespace NetCorePal.D3Shop.Web.Blazor.Services;

public class RolesService(RoleController roleController) : IRolesService
{
    public async Task<ResponseData<RoleId>> CreateRole(CreateRoleRequest request)
    {
        return await roleController.CreateRole(request);
    }

    public async Task<ResponseData<IEnumerable<RoleResponse>>> GetAllRoles()
    {
        return await roleController.GetAllRoles();
    }

    public async Task<ResponseData> UpdateRoleInfo(RoleId id, UpdateRoleInfoRequest request)
    {
        return await roleController.UpdateRoleInfo(id, request);
    }

    public async Task<ResponseData> UpdateRolePermissions(RoleId id, List<string> permissionCodes)
    {
        return await roleController.UpdateRolePermissions(id, permissionCodes);
    }

    public async Task<ResponseData> DeleteRole(RoleId id)
    {
        return await roleController.DeleteRole(id);
    }

    public async Task<ResponseData<IEnumerable<RoleResponse>>> GetRolesByCondition(
        RoleQueryRequest request)
    {
        return await roleController.GetRolesByCondition(request);
    }

    public async Task<ResponseData<RoleResponse>> GetRoleById(RoleId id)
    {
        return await roleController.GetRoleById(id);
    }

    public async Task<ResponseData<IEnumerable<RolePermissionResponse>>>
        GetRolePermissions(RoleId id)
    {
        return await roleController.GetRolePermissions(id);
    }
}