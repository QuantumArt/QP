using Quantumart.QP8.CodeGeneration.Services;
using Quantumart.QP8.EntityFramework.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework.Services
{
    public class FileMappingConfigurator : QPDataContextMappingConfiguratorBase
    {
        private readonly ModelReader _model;
        public FileMappingConfigurator(string path)
            : base()
        {
            var model = new ModelReader(path, _ => { });
        }

        public override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
