Quantumart.QP8.Home = function (documentContext, siteElementId, searchElementId, lockedElementId, approvalElementId, loggedAsElementId, customerCode) {
  function initialize() {
    let $search = $(`#${searchElementId}`);
    $search.wrap($('<div/>', { id: `${searchElementId}_wrapper`, class: 'fieldWrapper group myClass' }));
    let $wrapper = $search.parent('div');
    let $form = $search.parents('form');
    $form.on('submit', onSubmit);
    let $div = $('<div/>', {
      id: `${searchElementId}_preview`,
      class: 'previewButton',
      title: $l.Home.search
    });
    $div.append($('<img/>', { src: '/Backend/Content/Common/0.gif' }));
    $wrapper.append($div);
    $div.on('click', onSubmit);
    let $locked = $(`#${lockedElementId}`);
    let $loggedAs = $(`#${loggedAsElementId}`);
    let $approval = $(`#${approvalElementId}`);
    let temp = ' (<a class="js" href="javascript:void(0)">{0}</a>) ';
    let listStr = String.format(temp, $l.Home.list);
    let profileStr = String.format(temp, $l.Home.profile);

    if ($locked.text().trim() != '0') {
      $locked.append(listStr);
      $locked.find('a').on('click', () => {
        executeAction('list_locked_article', 'db', 1, customerCode, 0);
      });
    }

    if ($approval.text().trim() != '0') {
      $approval.append(listStr);
      $approval.find('a').on('click', () => {
        executeAction('list_articles_for_approval', 'db', 1, customerCode, 0);
      });
    }

    $loggedAs.append(profileStr);
    $loggedAs.find('a').on('click', () => {
      executeAction('edit_profile', 'db', 1, customerCode, 0);
    });
  }

  function onSubmit(e) {
    e.preventDefault();
    let $site = $(`#${siteElementId}`);
    let siteId = $site.val();

    if (siteId) {
      let siteName = $site.text();
      let text = $(`#${searchElementId}`).val();
      executeAction('search_in_articles', 'site', siteId, siteName, 1, { query: text });
    }
  }

  function executeAction(actionCode, entityTypeCode, entityId, entityName, parentEntityId, additionalUrlParameters) {
    let action = $a.getBackendActionByCode(actionCode);

    let params = new Quantumart.QP8.BackendActionParameters({
      entityTypeCode: entityTypeCode,
      entityId: entityId,
      entityName: entityName,
      parentEntityId: parentEntityId
    });

    let eventArgs = $a.getEventArgsFromActionWithParams(action, params);
    eventArgs.set_context({ additionalUrlParameters: additionalUrlParameters });
    documentContext.getHost().onActionExecuting(eventArgs);
  }


  function dispose() {
    $(`#${searchElementId}`).siblings('.previewButton').off('click');
    $(`#${searchElementId}`).parents('form').off('sumbit');
    $(`#${lockedElementId}`).find('a').off('click');
    $(`#${loggedAsElementId}`).find('a').off('click');
    $(`#${approvalElementId}`).find('a').off('click');
  }

  return {
    dispose: dispose,
    initialize: initialize
  };
};
