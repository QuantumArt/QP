import { BackendEntityDataListManager } from '../Managers/BackendEntityDataListManager';
import { BackendEntityTreeManager } from '../Managers/BackendEntityTreeManager';

export class JoinContentAndJoinFieldsMediator {
  constructor(joinContentSelectElementId, joinFieldsTreeElementId) {
    const contentPicker = $(`#${joinContentSelectElementId}`).data('entity_data_list_component');
    const entityTreeComponent = BackendEntityTreeManager.getInstance().getTree(joinFieldsTreeElementId);

    const onRelatedToChanged = function () {
      const [selectedContentId] = contentPicker.getSelectedEntityIDs();
      entityTreeComponent.set_parentEntityId(selectedContentId);
      entityTreeComponent.set_selectedEntitiesIDs([]);
      entityTreeComponent.refreshTree();
    };

    const dispose = function () {
      $(contentPicker.getStateFieldElement()).off('change', onRelatedToChanged);
    };

    $(contentPicker.getStateFieldElement()).on('change', onRelatedToChanged);

    return {
      dispose
    };
  }
}

export class UnionRadioAndSourceContentsListMediator {
  constructor(
    unionSourcePanelElementId, buildParamsPanelElementId, unionSourcesElementId
  ) {
    const $unionSourcePanelElement = $(`#${unionSourcePanelElementId}`);
    const $buildParamsPanelElement = $(`#${buildParamsPanelElementId}`);
    const unionSourcesComponent = BackendEntityDataListManager.getInstance()
      .getList(`${unionSourcesElementId}_list`);

    const onUnionTypeSelected = function () {
      unionSourcesComponent._fixListOverflow();
    };

    const dispose = function () {
      $unionSourcePanelElement.unbind();
      $buildParamsPanelElement.unbind();
    };

    $unionSourcePanelElement.bind('show', onUnionTypeSelected);
    $buildParamsPanelElement.bind('show', onUnionTypeSelected);

    return { dispose };
  }
}

Quantumart.QP8.JoinContentAndJoinFieldsMediator = JoinContentAndJoinFieldsMediator;
Quantumart.QP8.UnionRadioAndSourceContentsListMediator = UnionRadioAndSourceContentsListMediator;
