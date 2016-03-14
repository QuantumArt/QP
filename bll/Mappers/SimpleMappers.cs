using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{               
	internal class SessionsLogMapper : GenericMapper<SessionsLog, SessionsLogDAL>
    {
    }	
    

    internal class FieldTypeMapper : GenericMapper<FieldType, FieldTypeDAL>
    {
    }
        

    internal class ArticleVersionMapper : GenericMapper<ArticleVersion, ArticleVersionDAL>
    {
    }
    

    internal class BackendActionViewMapper : GenericMapper<BackendActionView, ActionViewDAL>
    {
    }

    internal class ContentConstraintMapper : GenericMapper<ContentConstraint, ContentConstraintDAL>
    {
    }        	

	internal class MaskTemplateMapper : GenericMapper<MaskTemplate, MaskTemplateDAL>
	{
	}

	internal class BackendActionLogMapper : GenericMapper<BackendActionLog, BackendActionLogDAL>
	{
	}

	internal class ContextMenuMapper : GenericMapper<ContextMenu, ContextMenuDAL>
	{
	}

	internal class ContextMenuItemMapper : GenericMapper<ContextMenuItem, ContextMenuItemDAL>
	{
	}	

	internal class EntityPermissionLevelMapper : GenericMapper<EntityPermissionLevel, PermissionLevelDAL>
	{
	}			

    internal class NetLanguageMapper : GenericMapper<NetLanguage, NetLanguagesDAL>
    {
    }

    internal class LocaleMapper: GenericMapper<Locale, LocaleDAL>
    {
    }

    internal class CharsetMapper: GenericMapper<Charset, CharsetDAL>
    {
    }

	internal class ObjectTypeMapper: GenericMapper<ObjectType, ObjectTypeDAL>
	{
	}
	
	internal class ObjectFormatVersionMapper :GenericMapper<ObjectFormatVersion, ObjectFormatVersionDAL>
	{
	}

	internal class DbMapper : GenericMapper<Db, DbDAL>
	{
	}
}