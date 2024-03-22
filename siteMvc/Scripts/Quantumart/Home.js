import { $a, BackendActionParameters } from './BackendActionExecutor';

export class Home {
  // eslint-disable-next-line max-params
  constructor(
    documentContext,
    siteElementId,
    searchElementId,
    lockedElementId,
    approvalElementId,
    loggedAsElementId,
    customerCode,
    externalUserTaskElementId
  ) {
    const initialize = function () {
      // eslint-disable-next-line max-params
      const executeAction = function (
        actionCode,
        entityTypeCode,
        entityId,
        entityName,
        parentEntityId,
        additionalUrlParameters
      ) {
        const action = $a.getBackendActionByCode(actionCode);
        const params = new BackendActionParameters({
          entityTypeCode,
          entityId,
          entityName,
          parentEntityId
        });

        const eventArgs = $a.getEventArgsFromActionWithParams(action, params);
        eventArgs.set_context({ additionalUrlParameters });
        documentContext.getHost().onActionExecuting(eventArgs);
      };

      const onSubmit = function (e) {
        e.preventDefault();
        const $site = $(`#${siteElementId}`);
        const siteId = $site.val();

        if (siteId) {
          const siteName = $site.text();
          const text = $(`#${searchElementId}`).val();
          executeAction('search_in_articles', 'site', siteId, siteName, 1, { query: text });
        }
      };

      const $search = $(`#${searchElementId}`);
      $search.wrap($('<div/>', { id: `${searchElementId}_wrapper`, class: 'fieldWrapper group myClass' }));

      const $wrapper = $search.parent('div');
      const $form = $search.parents('form');
      $form.on('submit', onSubmit);

      const $div = $('<div/>', {
        id: `${searchElementId}_preview`,
        class: 'previewButton',
        title: $l.Home.search
      });

      $div.append($('<img/>', { src: 'Static/Common/0.gif' }));
      $wrapper.append($div);
      $div.on('click', onSubmit);

      const $locked = $(`#${lockedElementId}`);
      const $loggedAs = $(`#${loggedAsElementId}`);
      const $approval = $(`#${approvalElementId}`);
      const $userTasks = $(`#${externalUserTaskElementId}`);
      const temp = ' (<a class="js" href="javascript:void(0)">{0}</a>) ';
      const listStr = String.format(temp, $l.Home.list);
      const profileStr = String.format(temp, $l.Home.profile);

      if ($locked.text().trim() !== '0') {
        $locked.append(listStr);
        $locked.find('a').on('click', () => {
          executeAction('list_locked_article', 'db', 1, customerCode, 0);
        });
      }

      if ($userTasks.text().trim() !== '0') {
        $userTasks.append(listStr);
        $userTasks.find('a').on('click', () => {
          executeAction('list_article_external_workflow_tasks', 'db', 1, customerCode, 0);
        });
      }

      if ($approval.text().trim() !== '0') {
        $approval.append(listStr);
        $approval.find('a').on('click', () => {
          executeAction('list_articles_for_approval', 'db', 1, customerCode, 0);
        });
      }

      $loggedAs.append(profileStr);
      $loggedAs.find('a').on('click', () => {
        executeAction('edit_profile', 'db', 1, customerCode, 0);
      });
    };

    const dispose = function () {
      $(`#${searchElementId}`).siblings('.previewButton').off('click');
      $(`#${searchElementId}`).parents('form').off('sumbit');
      $(`#${lockedElementId}`).find('a').off('click');
      $(`#${loggedAsElementId}`).find('a').off('click');
      $(`#${approvalElementId}`).find('a').off('click');
      $(`#${externalUserTaskElementId}`).find('a').off('click');
    };

    return {
      dispose,
      initialize
    };
  }
}

Quantumart.QP8.Home = Home;
