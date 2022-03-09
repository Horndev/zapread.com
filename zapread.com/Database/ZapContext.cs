using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text.RegularExpressions;
using zapread.com.Models;
using zapread.com.Models.Database;
using zapread.com.Models.Lightning;

namespace zapread.com.Database
{
    /// <summary>
    /// Main Database Context
    /// </summary>
    public class ZapContext : DbContext
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ZapContext() : base("name=" + System.Configuration.ConfigurationManager.AppSettings["SiteConnectionString"])
        {
            //DbInterception.Add(FreetextInterceptor.Instance);
        }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Models.Database.Group> Groups { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserImage> Images { get; set; }
        public DbSet<LNTransaction> LightningTransactions { get; set; }
        public DbSet<EarningEvent> EarningEvents { get; set; }
        public DbSet<SpendingEvent> SpendingEvents { get; set; }
        public DbSet<ZapReadGlobals> ZapreadGlobals { get; set; }
        public DbSet<ZapIcon> Icons { get; set; }
        public DbSet<UserMessage> Messages { get; set; }
        public DbSet<UserAlert> Alerts { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<PendingPostVote> PendingPostVotes { get; set; }
        public DbSet<PendingCommentVote> PendingCommentVotes { get; set; }
        public DbSet<HourlyStatistics> HourlyStatistics { get; set; }
        public DbSet<DailyStatistics> DailyStatistics { get; set; }
        public DbSet<WeeklyStatistics> WeeklyStatistics { get; set; }
        public DbSet<MonthlyStatistics> MonthlyStatistics { get; set; }
        public DbSet<LNNode> LNNodes { get; set; }
        public DbSet<APIKey> APIKeys { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}

/// <summary>
/// 
/// </summary>
//public class FreetextConvention : IStoreModelConvention<EdmModel>
//{
//    public static readonly FreetextConvention Instance = new FreetextConvention();

//    public void Apply(EdmModel item, DbModel model)
//    {
//        var valueParameter = FunctionParameter.Create("column", this.GetStorePrimitiveType(model, PrimitiveTypeKind.String), ParameterMode.In);
//        var formatParameter = FunctionParameter.Create("value", this.GetStorePrimitiveType(model, PrimitiveTypeKind.String), ParameterMode.In);
//        var returnValue = FunctionParameter.Create("result", this.GetStorePrimitiveType(model, PrimitiveTypeKind.Boolean), ParameterMode.ReturnValue);

//        var function = this.CreateAndAddFunction(item, "FREETEXT", new[] { valueParameter, formatParameter }, new[] { returnValue });
//    }

//    protected EdmFunction CreateAndAddFunction(EdmModel item, String name, IList<FunctionParameter> parameters, IList<FunctionParameter> returnValues)
//    {
//        var payload = new EdmFunctionPayload { StoreFunctionName = name, Parameters = parameters, ReturnParameters = returnValues, Schema = this.GetDefaultSchema(item), IsBuiltIn = true };
//        var function = EdmFunction.Create(name, this.GetDefaultNamespace(item), item.DataSpace, payload, null);

//        item.AddItem(function);

//        return (function);
//    }

//    protected EdmType GetStorePrimitiveType(DbModel model, PrimitiveTypeKind typeKind)
//    {
//        return (model.ProviderManifest.GetStoreType(TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(typeKind))).EdmType);
//    }

//    protected String GetDefaultNamespace(EdmModel layerModel)
//    {
//        var ns = layerModel.GlobalItems.OfType<EdmType>()
//            .Select(t => t.NamespaceName)
//            .Distinct()
//            .Single();
//        return (ns);
//    }

//    protected String GetDefaultSchema(EdmModel layerModel)
//    {
//        return (layerModel.Container.EntitySets.Select(s => s.Schema).Distinct().SingleOrDefault());
//    }
//}

//public class FreetextInterceptor : IDbCommandInterceptor
//{
//    public static readonly FreetextInterceptor Instance = new FreetextInterceptor();

//    private static readonly Regex FreetextRegex = new Regex(@"FREETEXT\(([^)]+\))\) = 1");

//    public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<Int32> interceptionContext)
//    {
//    }

//    public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<Int32> interceptionContext)
//    {
//    }

//    public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
//    {
//    }

//    public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
//    {
//        var matches = FreetextRegex.Matches(command.CommandText);

//        if (matches.Count > 0)
//        {
//            command.CommandText = FreetextRegex.Replace(command.CommandText, "FREETEXT($1)");
//        }
//    }

//    public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<Object> interceptionContext)
//    {
//    }

//    public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<Object> interceptionContext)
//    {
//    }
//}

/// <summary>
/// Inspired by
/// 
/// https://weblogs.asp.net/ricardoperes/registering-sql-server-built-in-functions-to-entity-framework-code-first
/// 
/// and
/// 
/// https://weblogs.asp.net/ricardoperes/freetext-extension-in-entity-framework-code-first
/// </summary>
//public class RegisterFunctionConvention : IStoreModelConvention<EdmModel>
//{
//    public void Apply(EdmModel item, DbModel model)
//    {
//        var valueParameter = FunctionParameter.Create("value", this.GetStorePrimitiveType(model, PrimitiveTypeKind.DateTime), ParameterMode.In);
//        var formatParameter = FunctionParameter.Create("format", this.GetStorePrimitiveType(model, PrimitiveTypeKind.String), ParameterMode.In);
//        var cultureParameter = FunctionParameter.Create("culture", this.GetStorePrimitiveType(model, PrimitiveTypeKind.String), ParameterMode.In);
//        var returnValue = FunctionParameter.Create("result", this.GetStorePrimitiveType(model, PrimitiveTypeKind.String), ParameterMode.ReturnValue);

//        var function = this.CreateAndAddFunction(item, "FORMAT", new[] { valueParameter, formatParameter, cultureParameter }, new[] { returnValue });
//    }

//    protected EdmFunction CreateAndAddFunction(EdmModel item, String name, IList<FunctionParameter> parameters, IList<FunctionParameter> returnValues)
//    {
//        var payload = new EdmFunctionPayload { StoreFunctionName = name, Parameters = parameters, ReturnParameters = returnValues, Schema = this.GetDefaultSchema(item), IsBuiltIn = true };
//        var function = EdmFunction.Create(name, this.GetDefaultNamespace(item), item.DataSpace, payload, null);

//        item.AddItem(function);

//        return (function);
//    }

//    protected EdmType GetStorePrimitiveType(DbModel model, PrimitiveTypeKind typeKind)
//    {
//        return (model.ProviderManifest.GetStoreType(TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(typeKind))).EdmType);
//    }

//    protected String GetDefaultNamespace(EdmModel layerModel)
//    {
//        return (layerModel.GlobalItems.OfType<EdmType>().Select(t => t.NamespaceName).Distinct().Single());
//    }

//    protected String GetDefaultSchema(EdmModel layerModel)
//    {
//        return (layerModel.Container.EntitySets.Select(s => s.Schema).Distinct().SingleOrDefault());
//    }

//    [DbFunction("CodeFirstDatabaseSchema", "FREETEXT")]
//    public static Boolean Freetext(this String column, String value)
//    {
//        return column.Contains(value);
//    }
//}