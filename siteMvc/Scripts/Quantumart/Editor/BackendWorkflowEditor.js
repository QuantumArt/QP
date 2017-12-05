/* eslint new-cap: 0 */
/* eslint camelcase: 0 */
Quantumart.QP8.BackendWorkflow = function (componentElem) {
  this._componentElem = componentElem;
  this._containerElem = $('.workflowContainer', componentElem);
  this._checkSinglePermisssionHandler = $.proxy(this.checkSinglePermission, this);
  this._checkAllPermisssionsHandler = $.proxy(this.checkAllPermisssions, this);
};

Quantumart.QP8.BackendWorkflow.prototype = {
  _componentElem: null,
  _resultElem: null,
  _containerElem: null,
  _addItemHandler: null,
  _removeItemHandler: null,
  _tableHeader: null,
  _tableBody: null,
  _items: null,
  _contentSelector: null,
  _checkSinglePermisssionHandler: null,
  _checkAllPermisssionsHandler: null,

  initialize() {
    const workflow = this._componentElem;
    this._items = ko.observableArray(
      workflow.data('workflow_list_data').map(data => Object.assign({}, data, {
        RadioChecked: ko.observable(data.RadioChecked),
        Description: ko.observable(data.Description),
        UserId: ko.observable(data.UserId),
        GroupId: ko.observable(data.GroupId)
      }))
    );

    this._contentSelector = this._componentElem.closest('form').find('.workflow_content_selector');
    const viewModel = {
      items: this._items,
      contentSelector: this._contentSelector,
      checkSinglePermisssionHandler: this._checkSinglePermisssionHandler,
      initializePickers(dom, element) {
        Quantumart.QP8.ControlHelpers.initAllEntityDataLists($(dom));
        $(dom)
          .find('.pep-user-selector')
          .first()
          .data('entity_data_list_component')
          .selectEntities([element.UserId()]);

        $(dom)
          .find('.pep-user-selector')
          .last()
          .data('entity_data_list_component')
          .selectEntities([element.GroupId()]);

        $(dom)
          .find(`.${window.CHANGED_FIELD_CLASS_NAME}`)
          .removeClass(window.CHANGED_FIELD_CLASS_NAME);

        const activeContentsIds = this.contentSelector
          .find('input:checkbox:checked')
          .map((index, elem) => $(elem).val())
          .get()
          .join();

        if ($q.isNull(element.UserId()) && $q.isNull(element.GroupId())) {
          $(dom).find('.singleItemPicker').each(jQuery.proxy(function (index, elem) {
            $(elem).data('entity_data_list_component').attachObserver(
              window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, this.checkSinglePermisssionHandler
            );
          }, this));
        } else {
          $q.getJsonFromUrl(
            'GET',
            `${window.CONTROLLER_URL_WORKFLOW}CheckUserOrGroupAccessOnContents`,
            {
              contentIdsString: activeContentsIds,
              statusName: element.StName,
              userIdString: element.UserId,
              groupIdString: element.GroupId
            },
            false,
            false,
            data => {
              if ($q.isNull(element.UserId())) {
                $(dom).find('span.workflow_permission_message').last().html(data);
              } else {
                $(dom).find('span.workflow_permission_message').first().html(data);
              }
            }
          );
        }
      },

      disposePickers(dom) {
        Quantumart.QP8.ControlHelpers.destroyAllEntityDataLists($(dom));
        $(dom).remove();
      }
    };

    ko.applyBindingsToNode(
      this._containerElem.get(0),
      { template: { name: workflow.attr('id').replace('_workflow_control', '_template') } }, viewModel
    );
    this._resultElem = $('.workflowResult', this._componentElem);

    const that = this;
    this._componentElem.closest('form').find('.workflow_control_selector').parent('div').find('.checkbox')
      .change(e => {
        that.manageItems(e);
      });

    this._contentSelector
      .data('entity_data_list_component').attachObserver(
        window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, this._checkAllPermisssionsHandler
      );

    this._containerElem.find('.singleItemPicker').each(jQuery.proxy(function (index, elem) {
      $(elem).data('entity_data_list_component').attachObserver(
        window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, this._checkSinglePermisssionHandler
      );
    }, this));

    const onRadioChanged = function () {
      if ($(this).val() === 'User') {
        $(this).closest('fieldset').find('.workflow_group_row').hide();
        $(this).closest('fieldset').find('.workflow_user_row').show();
      } else {
        $(this).closest('fieldset').find('.workflow_user_row').hide();
        $(this).closest('fieldset').find('.workflow_group_row').show();
      }
    };

    $('.workflow_radio').live('click', onRadioChanged);
    this._componentElem.data('workflow', this);
  },

  getCheckedContentsIds() {
    return this._contentSelector.find('input:checkbox:checked').map((index, elem) => $(elem).val()).get().join();
  },

  checkAllPermisssions() {
    const activeContentsIds = this.getCheckedContentsIds();
    const usersAndGroups = this._items().map(elem => ({
      StName: elem.StName,
      UserId: elem.UserId(),
      GroupId: elem.GroupId()
    }));

    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_WORKFLOW}CheckAllAccessOnContents`,
      {
        contentIdsString: activeContentsIds,
        modelString: JSON.stringify(usersAndGroups)
      },
      false,
      false,
      $.proxy(function (data) {
        this._containerElem.find('span.workflow_permission_message').html('');
        Object.keys(data).forEach(i => {
          const current_workflow_stage = this._containerElem.find(`.${data[i].StName}`);
          const user_row = current_workflow_stage.find(':visible.workflow_user_row');
          const group_row = current_workflow_stage.find(':visible.workflow_group_row');

          if (user_row.size() > group_row.size()) {
            const span = user_row.find('span.workflow_permission_message');
            const oldHtml = span.html();
            span.html(`${oldHtml}<br>${data[i].Message}`);
          } else {
            const span = group_row.find('span.workflow_permission_message');
            const oldHtml = span.html();
            group_row.find('span.workflow_permission_message').html(`${oldHtml}<br>${data[i].Message}`);
          }
        }, this);
      }, this)
    );
  },

  checkSinglePermission(eventType, eventArgs) {
    let userId, groupId;
    const activeContentsIds = this.getCheckedContentsIds();
    const statusName = $(eventArgs._listElement).closest('fieldset').attr('Class').replace('workflow_fieldset', '');

    if (eventArgs.get_entityTypeCode() === 'user') {
      userId = $q.isNullOrEmpty(eventArgs.getSelectedEntities()[0]) ? null : eventArgs.getSelectedEntities()[0].Id;
    } else if (eventArgs.get_entityTypeCode() === 'user_group') {
      groupId = $q.isNullOrEmpty(eventArgs.getSelectedEntities()[0]) ? null : eventArgs.getSelectedEntities()[0].Id;
    }
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_WORKFLOW}CheckUserOrGroupAccessOnContents`,
      {
        contentIdsString: activeContentsIds,
        statusName,
        userIdString: userId,
        groupIdString: groupId
      },
      false,
      false,
      data => {
        const stage = $(eventArgs._listElement).closest('fieldset');
        const userRow = stage.find(':visible.workflow_user_row');
        const groupRow = stage.find(':visible.workflow_group_row');
        if (userRow.size() > groupRow.size()) {
          userRow.find('span.workflow_permission_message').html(data);
        } else {
          groupRow.find('span.workflow_permission_message').html(data);
        }
      }
    );
  },

  manageItems(e) {
    let target = $(e.target);
    if (target.size() === 0) {
      target = $(e);
    }

    if (target.parent('div').hasClass('groupCheckbox')) {
      target
        .parent('.groupCheckbox')
        .siblings('.workflow_control_selector')
        .find('.checkbox')
        .each(jQuery.proxy(function (index, item) {
          this.manageItems($(item));
        }, this));
    } else if (target.attr('checked') === 'checked') {
      this.addItem(
        target.val(),
        target.siblings('label').text(),
        target.closest('.workflow_control_selector').data('weights')[target.val()]
      );
    } else {
      this.removeItem(target.val());
    }
  },

  addItem(statusId, statusName, weight) {
    const existingItem = ko.utils.arrayFirst(this._items(), item => item.StId === statusId);

    if ($q.isNull(existingItem)) {
      const item = {
        StName: statusName,
        StId: statusId,
        RadioChecked: ko.observable('User'),
        UserId: ko.observable(null),
        GroupId: ko.observable(null),
        Description: ko.observable('(no comments)'),
        Weight: weight,
        Invalid: false
      };

      this._items.push(item);
      this._items.sort((left, right) => {
        if (left.Weight === right.Weight) {
          return 0;
        } else if (left.Weight > right.Weight) {
          return 1;
        }
        return -1;
      });
      this._setAsChanged();
    }
  },

  removeItem(statusId) {
    const items = this._items();
    for (let i = 0; i < items.length; i++) {
      const current = items[i];
      if (current.StId === statusId) {
        this._items.remove(current);
      }
    }
    this._setAsChanged();
  },

  _setAsChanged() {
    const $field = $(this._resultElem);
    $field.addClass(window.CHANGED_FIELD_CLASS_NAME);
    $field.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { fieldName: $field.attr('name'), value: this._items() });
  },

  destroyWorkflow() {
    const containerElem = this._containerElem;
    containerElem.find('.singleItemPicker').each(jQuery.proxy(function (index, elem) {
      $(elem).data('entity_data_list_component').detachObserver(
        window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, this._checkSinglePermisssionHandler
      );
    }), this);

    this._contentSelector
      .data('entity_data_list_component')
      .detachObserver(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, this._checkAllPermisssionsHandler);

    ko.cleanNode(containerElem.get(0));
  }
};
