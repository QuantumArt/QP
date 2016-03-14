QP_CURRENT_CONTEXT.resolverNames = ["Type", "Product"];
QP_CURRENT_CONTEXT.parentTypeFieldName = "Type";
QP_CURRENT_CONTEXT.typeResolverInputName = 'field_1374';
QP_CURRENT_CONTEXT.productResolverInputName = 'field_1374';
QP_CURRENT_CONTEXT.typeFilters = {
	"field_1375" : "",
	"field_1379" : "",
	"field_1380" : "",
	"field_1758" : "",
	"field_1506" : "",
};
QP_CURRENT_CONTEXT.productFilters = {
  "field_1642" : "Product"
};

QP_CURRENT_CONTEXT.changeFieldState = function(editor, data, $rootElem)
{
	if (!$rootElem || $rootElem.prop("tagName") == "FORM")
	{
      	if (!data || data.fieldName == "field_1374")
        {
			this.toggleField(editor, "field_1739", this.getValue(editor, "field_1374") == "", !data);
        }
      
      	if (!data || data.fieldName == "field_1739")
        {
			this.toggleField(editor, "field_1374", this.getValue(editor, "field_1739") == "", !data);
        }

      	if (data && data.fieldName == "field_1375")
        {
			if (this.getValue(editor, "field_1373") == "")
            {
             	this.setValue(editor, "field_1373", this.getText(editor, "field_1375"));
            }
        }      
      
	}
};