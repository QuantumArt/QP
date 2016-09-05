using EntityFramework6.Test.DataContext;
using EntityFramework6.Test.Infrastructure;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework6.Test.Tests.UpdateContentData
{
    [TestFixture]
    public class UpdateFileFieldFixture : DataContextUpdateFixtureBase
    {
        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_File_Field_isUpdated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            UpdateProperty<FileItemForUpdate>(access, mapping, a => a.FileValueField = $"{Guid.NewGuid()}.txt", a => a.FileValueField);
        }

        [Test, Combinatorial]
        [Category("UpdateContentData")]
        public void Check_That_Date_Image_isUpdated([ContentAccessValues] ContentAccess access, [MappingValues] Mapping mapping)
        {
            UpdateProperty<ImageItemForUpdate>(access, mapping, a => a.ImageValueField = $"{Guid.NewGuid()}.gif", a => a.ImageValueField);
        }
    }
}