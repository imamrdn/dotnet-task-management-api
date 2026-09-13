using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Extensions;

public static class TaskQueryExtensions
{
    public static IQueryable<TaskItem> WhereActive(this IQueryable<TaskItem> query)
    {
        return query.Where(task => !task.IsDeleted);
    }
}
