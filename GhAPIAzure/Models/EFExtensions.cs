namespace EFExtensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    public static class EFExtensions
    {
        public static EntityOp<TEntity> Upsert<TEntity>(this DbContext context, TEntity entity) where TEntity : class
        {
            return new UpsertOp<TEntity>(context, entity);
        }
    }

    public abstract class EntityOp<TEntity, TRet>
    {
        public readonly DbContext Context;
        public readonly TEntity Entity;

        private readonly List<string> excludeProperties = new List<string>();

        private readonly List<string> keyNames = new List<string>();
        public readonly string TableName;

        public EntityOp(DbContext context, TEntity entity)
        {
            Context = context;
            Entity = entity;

            var mappingAttrs = typeof(TEntity).GetCustomAttributes(typeof(TableAttribute), false);
            TableAttribute tableAttr = null;
            if (mappingAttrs.Length > 0)
            {
                tableAttr = mappingAttrs[0] as TableAttribute;
            }

            if (tableAttr == null)
            {
                throw new ArgumentException("TEntity is missing TableAttribute", "entity");
            }

            TableName = tableAttr.Name;
        }

        public IEnumerable<PropertyInfo> ColumnProperties
        {
            get { return typeof(TEntity).GetProperties().Where(pr => !excludeProperties.Contains(pr.Name)); }
        }

        public IEnumerable<string> KeyNames { get { return keyNames; } }

        public EntityOp<TEntity, TRet> ExcludeField<TField>(Expression<Func<TEntity, TField>> selectField)
        {
            excludeProperties.Add(GetMemberName(selectField));
            return this;
        }

        public abstract TRet Execute();

        public EntityOp<TEntity, TRet> Key<TKey>(Expression<Func<TEntity, TKey>> selectKey)
        {
            keyNames.Add(GetMemberName(selectKey));
            return this;
        }

        public void Run() { Execute(); }

        private static string GetMemberName<T>(Expression<Func<TEntity, T>> selectMemberLambda)
        {
            var member = selectMemberLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException(
                    "The parameter selectMemberLambda must be a member accessing labda such as x => x.Id",
                    "selectMemberLambda");
            }
            return member.Member.Name;
        }
    }

    public abstract class EntityOp<TEntity> : EntityOp<TEntity, int>
    {
        public EntityOp(DbContext context, TEntity entity) : base(context, entity) { }

        public sealed override int Execute()
        {
            ExecuteNoRet();
            return 0;
        }

        protected abstract void ExecuteNoRet();
    }

    public class UpsertOp<TEntity> : EntityOp<TEntity>
    {
        public UpsertOp(DbContext context, TEntity entity) : base(context, entity) { }

        protected override void ExecuteNoRet()
        {
            var sql = new StringBuilder();

            var notNullFields = 0;
            var valueKeyList = new List<string>();
            var columnList = new List<string>();
            var valueList = new List<object>();
            foreach (var p in ColumnProperties)
            {
                columnList.Add(p.Name);
                var val = p.GetValue(Entity, null);
                if (val != null)
                {
                    valueKeyList.Add("{" + notNullFields++ + "}");
                    valueList.Add(val);
                }
                else
                {
                    valueKeyList.Add("null");
                }
            }
            var columns = columnList.ToArray();

            sql.Append("merge into ");
            sql.Append(TableName);
            sql.Append(" as T ");

            sql.Append("using (values (");
            sql.Append(string.Join(",", valueKeyList.ToArray()));
            sql.Append(")) as S (");
            sql.Append(string.Join(",", columns));
            sql.Append(") ");

            sql.Append("on (");
            var mergeCond = string.Join(" and ", KeyNames.Select(kn => "T." + kn + "=S." + kn));
            sql.Append(mergeCond);
            sql.Append(") ");

            sql.Append("when matched then update set ");
            sql.Append(string.Join(",", columns.Select(c => "T." + c + "=S." + c).ToArray()));

            sql.Append(" when not matched then insert (");
            sql.Append(string.Join(",", columns));
            sql.Append(") values (S.");
            sql.Append(string.Join(",S.", columns));
            sql.Append(");");

            Context.Database.ExecuteSqlCommand(sql.ToString(), valueList.ToArray());
        }
    }
}
