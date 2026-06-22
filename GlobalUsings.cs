global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Linq.Expressions;
global using System.Text.Json;
global using System.Text.Json.Serialization;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Logging;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;
global using Microsoft.AspNetCore.Hosting;

global using AutoMapper;
global using FluentValidation;

global using LMS_SoulCode.Features.Common;
global using LMS_SoulCode.Features.Common.Models;
global using LMS_SoulCode.Features.Common.Repositories;
global using LMS_SoulCode.Features.Common.Pagination;
global using LMS_SoulCode.Data;
