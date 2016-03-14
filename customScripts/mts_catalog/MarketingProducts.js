QP_CURRENT_CONTEXT.typeResolverInputName = 'field_1540';
QP_CURRENT_CONTEXT.typeFilters = {
	"field_1537" : "",
	"field_1809" : "",
  	"field_1653" : ""
};

QP_CURRENT_CONTEXT.changeFieldState = function(editor, data, $rootElem)
{
	if (!$rootElem || $rootElem.prop("tagName") == "FORM")
	{
       if (!data || data.fieldName == "field_1540")
        {
			this.toggleField(editor, "field_1753", this.getValue(editor, "field_1540") == "402", !data);
			this.toggleField(editor, "field_1755", this.getValue(editor, "field_1540") == "402", !data);          
        }
      
      	if (!data)
        {
 			var $form = $rootElem || jQuery(editor._formElement);
          	this.applyFilter("field_1757", "c.Product is null", $form)
         	
        }
      
	}
};