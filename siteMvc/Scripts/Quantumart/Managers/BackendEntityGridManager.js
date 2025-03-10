import { BackendEntityGrid } from '../BackendEntityGrid';
import { Observable } from '../Common/Observable';
import { $o } from '../Info/BackendEntityObject';
import { $q } from '../Utils';

export class BackendEntityGridManager extends Observable {
  /** @type {BackendEntityGridManager} */
  static _instance;

  static getInstance() {
    if (!BackendEntityGridManager._instance) {
      BackendEntityGridManager._instance = new BackendEntityGridManager();
    }

    return BackendEntityGridManager._instance;
  }

  static destroyInstance() {
    if (BackendEntityGridManager._instance) {
      BackendEntityGridManager._instance.dispose();
      BackendEntityGridManager._instance = null;
    }
  }

  static generateGridGroupCode(entityTypeCode, parentEntityId) {
    return parentEntityId ? `${entityTypeCode}_${parentEntityId}` : entityTypeCode;
  }

  constructor() {
    super();
    /** @type {{ [x: string]: { [x: string]: BackendEntityGrid } }} */
    this._gridGroups = {};
  }

  getGridGroup(gridGroupCode) {
    return this._gridGroups[gridGroupCode];
  }

  createGridGroup(gridGroupCode) {
    this._gridGroups[gridGroupCode] = this.getGridGroup(gridGroupCode) || {};
    return this._gridGroups[gridGroupCode];
  }

  refreshGridGroup(entityTypeCode, parentEntityId, options) {
    const gridGroup = this.getGridGroup(BackendEntityGridManager.generateGridGroupCode(entityTypeCode, parentEntityId));
    if (gridGroup) {
      Object.keys(gridGroup).forEach(gridElementId => {
        this.refreshGrid(gridElementId, options);
      }, this);
    }
  }

  refreshGridGroupWithChecking(entityTypeCode, parentEntityId, entityId) {
    const gridGroup = this.getGridGroup(BackendEntityGridManager.generateGridGroupCode(entityTypeCode, parentEntityId));
    if (gridGroup) {
      Object.keys(gridGroup).forEach(gridElementId => {
        const grid = this.getGrid(gridElementId);
        if (grid && grid.checkExistEntityInCurrentPage(entityId)) {
          grid.refreshGrid();
        }
      }, this);
    }
  }

  resetGridGroup(gridGroupCode, options) {
    const gridGroup = this.getGridGroup(gridGroupCode);
    if (gridGroup) {
      Object.keys(gridGroup).forEach(gridElementId => {
        this.resetGrid(gridElementId, options);
      }, this);
    }
  }

  removeGridGroup(gridGroupCode) {
    $q.removeProperty(this._gridGroups, gridGroupCode);
  }

  /**
   * @param {string} gridElementId
   * @returns {BackendEntityGrid}
   */
  getGrid(gridElementId) {
    const gridGroup = Object.values(this._gridGroups).find(val => Boolean(val[gridElementId]));
    return gridGroup[gridElementId];
  }

  // eslint-disable-next-line max-params
  createGrid(gridElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions) {
    const gridGroupCode = BackendEntityGridManager.generateGridGroupCode(entityTypeCode, parentEntityId);
    const actionGroupCode = BackendEntityGridManager.generateGridGroupCode(actionCode, parentEntityId);
    const actionSimpleGroupCode = BackendEntityGridManager.generateGridGroupCode(actionCode);

    const grid = new BackendEntityGrid([
      gridGroupCode,
      actionGroupCode,
      actionSimpleGroupCode
    ], gridElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions);

    grid.set_gridManager(this);
    let gridGroup = this.createGridGroup(gridGroupCode);
    gridGroup[gridElementId] = grid;

    gridGroup = this.createGridGroup(actionGroupCode);
    gridGroup[gridElementId] = grid;

    gridGroup = this.createGridGroup(actionSimpleGroupCode);
    gridGroup[gridElementId] = grid;

    return grid;
  }

  refreshGrid(gridElementId, options) {
    const grid = this.getGrid(gridElementId);
    if (grid) {
      grid.refreshGrid(options);
    }
  }

  resetGrid(gridElementId, options) {
    const grid = this.getGrid(gridElementId);
    if (grid) {
      grid.resetGrid(options);
    }
  }

  removeGrid(gridElementId) {
    const grid = this.getGrid(gridElementId);
    if (grid) {
      const gridGroupCodes = grid.get_gridGroupCodes();
      const that = this;
      $.each(gridGroupCodes, (i, gridGroupCode) => {
        const gridGroup = that.getGridGroup(gridGroupCode);
        $q.removeProperty(gridGroup, gridElementId);
        if ($q.getHashKeysCount(gridGroup) === 0) {
          that.removeGridGroup(gridGroupCode);
        }
      });
    }
  }

  // eslint-disable-next-line complexity
  onActionExecuted(eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    if (entityTypeCode !== window.ENTITY_TYPE_CODE_SITE_FILE
      && entityTypeCode !== window.ENTITY_TYPE_CODE_CONTENT_FILE
    ) {
      let parentEntityId = eventArgs.get_parentEntityId();
      const actionTypeCode = eventArgs.get_actionTypeCode();
      const actionCode = eventArgs.get_actionCode();
      const entityId = eventArgs.get_entityId();
      if (eventArgs.get_isSaved()
        || actionTypeCode === window.ACTION_TYPE_CODE_COPY
        || actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_UNLOCK
        || eventArgs.get_isRemoving()
        || eventArgs.get_isArchiving()
        || eventArgs.get_isRestoring()
        || actionCode === window.ACTION_CODE_MULTIPLE_PUBLISH_ARTICLES) {
        let options = null;

        if (eventArgs.get_isArchiving()
          || eventArgs.get_isRestoring()
          || eventArgs.get_isRemoving()
          || actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_UNLOCK
        ) {
          const removedIds = eventArgs.get_isMultipleEntities()
            ? $o.getEntityIDsFromEntities(eventArgs.get_entities())
            : [eventArgs.get_entityId()];

          options = { removedIds };
        }

        this.refreshGridGroup(entityTypeCode, parentEntityId, options);
      } else if (eventArgs.get_isUpdated()
        || eventArgs.get_isLoaded()
        || actionTypeCode === window.ACTION_TYPE_CODE_CANCEL
        || actionTypeCode === window.ACTION_TYPE_CODE_CHANGE_LOCK) {
        this.refreshGridGroupWithChecking(entityTypeCode, parentEntityId, entityId);
      }

      if (eventArgs.get_isUpdated() && entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE) {
        this.refreshGridGroup(window.ENTITY_TYPE_CODE_ARTICLE_VERSION, entityId);
      } else if ((eventArgs.get_isArchiving() || eventArgs.get_isRemoving())
        && entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE
      ) {
        this.refreshGridGroup(window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE, parentEntityId);
      } else if (eventArgs.get_isRestoring() && entityTypeCode === window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE) {
        this.refreshGridGroup(window.ENTITY_TYPE_CODE_ARTICLE, parentEntityId);
      } else if (eventArgs.get_isRestored() && entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
        parentEntityId = +$o.getParentEntityId(window.ENTITY_TYPE_CODE_ARTICLE, entityId) || 0;
        this.refreshGridGroup(window.ENTITY_TYPE_CODE_ARTICLE, parentEntityId);
      } else if (actionTypeCode === window.ACTION_TYPE_CHILD_ENTITY_PERMISSION_SAVE
        || actionTypeCode === window.ACTION_TYPE_CHILD_ENTITY_MULTIPLE_REMOVE
        || actionTypeCode === window.ACTION_TYPE_CHILD_ENTITY_REMOVE_ALL
        || actionTypeCode === window.ACTION_TYPE_CHILD_ENTITY_REMOVE
      ) {
        if (entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_PERMISSION) {
          this.refreshGridGroup(window.ACTION_CODE_CHILD_CONTENT_PERMISSIONS, parentEntityId);
          this.refreshGridGroup(window.ACTION_CODE_CONTENT_PERMISSIONS);
        } else if (entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE_PERMISSION) {
          this.refreshGridGroup(window.ACTION_CODE_CHILD_ARTICLE_PERMISSIONS, parentEntityId);
          this.refreshGridGroup(window.ACTION_CODE_ARTICLE_PERMISSIONS);
        }
      } else if (eventArgs.get_isSaved() || eventArgs.get_isUpdated() || eventArgs.get_isRemoving()) {
        if (entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT_PERMISSION) {
          this.refreshGridGroup(window.ACTION_CODE_CHILD_CONTENT_PERMISSIONS);
        } else if (entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE_PERMISSION) {
          this.refreshGridGroup(window.ACTION_CODE_CHILD_ARTICLE_PERMISSIONS);
        }
      } else if (actionCode === window.ACTION_CODE_COMPLETE_EXTERNAL_WORKFLOW_TASK) {
        this.refreshGridGroup(window.ACTION_CODE_EXTERNAL_WORKFLOW_TASKS);
      }
    }
  }

  dispose() {
    super.dispose();
    if (this._gridGroups) {
      Object.values(this._gridGroups).forEach(gridGroup => {
        Object.keys(gridGroup).forEach(gridElementId => {
          const grid = this.getGrid(gridElementId);
          if (grid && grid.dispose) {
            grid.dispose();
          }
        }, this);
      }, this);
    }

    this._gridGroups = null;
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendEntityGridManager = BackendEntityGridManager;
