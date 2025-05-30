﻿using NetCorePal.D3Shop.Domain.AggregatesModel.Identity.AdminUserAggregate;
using NetCorePal.D3Shop.Domain.AggregatesModel.Identity.DepartmentAggregate;
using NetCorePal.D3Shop.Infrastructure.Repositories.Identity.Admin;
using NetCorePal.Extensions.Primitives;

namespace NetCorePal.D3Shop.Web.Application.Commands.Identity.Admin;

public record UpdateUserDeptInfoCommand(AdminUserId AdminUserId, DeptId DeptId, string DeptName) : ICommand;

public class UpdateUserDeptInfoCommandHandler(AdminUserRepository adminUserRepository)
    : ICommandHandler<UpdateUserDeptInfoCommand>
{
    public async Task Handle(UpdateUserDeptInfoCommand request, CancellationToken cancellationToken)
    {
        var user = await adminUserRepository.GetAsync(request.AdminUserId, cancellationToken) ??
                   throw new KnownException($"未找到用户，AdminUserId = {request.AdminUserId}");

        user.SetUserDepts(request.DeptId, request.DeptName);
    }
}