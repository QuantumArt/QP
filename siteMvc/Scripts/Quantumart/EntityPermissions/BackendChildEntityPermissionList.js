import { BackendUserAndGroupSearchBlock } from '../Search/BackendUserAndGroupSearchBlock';
import { BackendEntityGridManager } from '../Managers/BackendEntityGridManager';

export class BackendChildEntityPermissionList {
  constructor(searchBlockElementId, mainComponentElementId) {
    const $searchBlock = jQuery(`#${searchBlockElementId}`);
    const beGrid = BackendEntityGridManager.getInstance().getGrid(mainComponentElementId);
    const $grid = jQuery('.pep-grid', $searchBlock);

    const onApplyFilter = function () {
      beGrid.resetGrid();
    };

    const searchBlockComponent = new BackendUserAndGroupSearchBlock(
      searchBlockElementId,
      $.proxy(onApplyFilter, this)
    );

    const dispose = function () {
      searchBlockComponent.dispose();
      $grid.off();
    };

    beGrid._createDataQueryParams = () => ({
      gridParentId: beGrid._parentEntityId,
      ...searchBlockComponent.getSearchData()
    });

    const modifyEventArgsContext = function (eventArgsContext) {
      return Object.assign(eventArgsContext || {}, { additionalUrlParameters: searchBlockComponent.getSearchData() });
    };

    return {
      dispose,
      modifyEventArgsContext
    };
  }
}

Quantumart.QP8.BackendChildEntityPermissionList = BackendChildEntityPermissionList;
