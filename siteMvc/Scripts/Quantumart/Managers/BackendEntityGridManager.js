Quantumart.QP8.BackendEntityGridManager = function () {
  Quantumart.QP8.BackendEntityGridManager.initializeBase(this);
};

Quantumart.QP8.BackendEntityGridManager.prototype = {
  _gridGroups: {},

  generateGridGroupCode: function (entityTypeCode, parentEntityId) {
    if (parentEntityId) {
      return String.format('{0}_{1}', entityTypeCode, parentEntityId);
    }
      return entityTypeCode;

  },

  getGridGroup: function (gridGroupCode) {
    var gridGroup = null;

    if (this._gridGroups[gridGroupCode]) {
      gridGroup = this._gridGroups[gridGroupCode];
    }

    return gridGroup;
  },

  createGridGroup: function (gridGroupCode) {
    var gridGroup = this.getGridGroup(gridGroupCode);

    if (!gridGroup) {
      gridGroup = {};
      this._gridGroups[gridGroupCode] = gridGroup;
    }

    return gridGroup;
  },

  refreshGridGroup: function (entityTypeCode, parentEntityId, options) {
    var gridGroup = this.getGridGroup(this.generateGridGroupCode(entityTypeCode, parentEntityId));

    if (gridGroup) {
      for (var gridElementId in gridGroup) {
        this.refreshGrid(gridElementId, options);
      }
    }

    gridGroup = null;
  },

  refreshGridGroupWithChecking: function (entityTypeCode, parentEntityId, entityId) {
    var gridGroup = this.getGridGroup(this.generateGridGroupCode(entityTypeCode, parentEntityId));

    if (gridGroup) {
      for (var gridElementId in gridGroup) {
        var grid = this.getGrid(gridElementId);

        if (grid && grid.checkExistEntityInCurrentPage(entityId)) {
          grid.refreshGrid();
        }
      }
    }
  },

  resetGridGroup: function (gridGroupCode, options) {
    var gridGroup = this.getGridGroup(gridGroupCode);

    if (gridGroup) {
      for (var gridElementId in gridGroup) {
        this.resetGrid(gridElementId, options);
      }
    }

    gridGroup = null;
  },

  removeGridGroup: function (gridGroupCode) {
    $q.removeProperty(this._gridGroups, gridGroupCode);
  },

  getGrid: function (gridElementId) {
    var grid = null;

    for (var gridGroupCode in this._gridGroups) {
      var gridGroup = this._gridGroups[gridGroupCode];

      if (gridGroup[gridElementId]) {
        grid = gridGroup[gridElementId];
        break;
      }
    }

    return grid;
  },

  createGrid: function (gridElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions) {
    var gridGroupCode = this.generateGridGroupCode(entityTypeCode, parentEntityId);
    var actionGroupCode = this.generateGridGroupCode(actionCode, parentEntityId);
    var actionSimpleGroupCode = this.generateGridGroupCode(actionCode);

    var grid = new Quantumart.QP8.BackendEntityGrid([
      gridGroupCode,
      actionGroupCode,
      actionSimpleGroupCode
    ], gridElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions);

    grid.set_gridManager(this);
    var gridGroup = this.createGridGroup(gridGroupCode);

    gridGroup[gridElementId] = grid;
    gridGroup = this.createGridGroup(actionGroupCode);
    gridGroup[gridElementId] = grid;
    gridGroup = this.createGridGroup(actionSimpleGroupCode);
    gridGroup[gridElementId] = grid;

    return grid;
  },

  refreshGrid: function (gridElementId, options) {
    var grid = this.getGrid(gridElementId);
    if (grid) {
      grid.refreshGrid(options);
    }
  },

  resetGrid: function (gridElementId, options) {
    var grid = this.getGrid(gridElementId);
    if (grid) {
      grid.resetGrid(options);
    }
  },

  removeGrid: function (gridElementId) {
    var grid = this.getGrid(gridElementId);

    if (grid) {
      var gridGroupCodes = grid.get_gridGroupCodes();
      var that = this;

      jQuery.each(gridGroupCodes, function (i, gridGroupCode) {
        var gridGroup = that.getGridGroup(gridGroupCode);

        $q.removeProperty(gridGroup, gridElementId);
        if ($q.getHashKeysCount(gridGroup) == 0) {
          that.removeGridGroup(gridGroupCode);
        }
      });
    }
  },

  destroyGrid: function (gridElementId) {
    var grid = this.getGrid(gridElementId);
    if (grid != null) {
      if (grid.dispose) {
        grid.dispose();
      }
    }
  },

  onActionExecuted: function (eventArgs) {
    var entityTypeCode = eventArgs.get_entityTypeCode();

    if (entityTypeCode != window.ENTITY_TYPE_CODE_SITE_FILE && entityTypeCode != window.ENTITY_TYPE_CODE_CONTENT_FILE) {
      var parentEntityId = eventArgs.get_parentEntityId();
      var actionTypeCode = eventArgs.get_actionTypeCode();
      var actionCode = eventArgs.get_actionCode();
      var entityId = eventArgs.get_entityId();

      if (eventArgs.get_isSaved()
        || actionTypeCode == window.ACTION_TYPE_CODE_COPY
        || actionTypeCode == window.ACTION_TYPE_CODE_MULTIPLE_UNLOCK
        || eventArgs.get_isRemoving()
        || eventArgs.get_isArchiving()
        || eventArgs.get_isRestoring()
        || actionCode == window.ACTION_CODE_MULTIPLE_PUBLISH_ARTICLES) {
        var options = null;

        if (eventArgs.get_isArchiving() || eventArgs.get_isRestoring() || eventArgs.get_isRemoving() || actionTypeCode == window.ACTION_TYPE_CODE_MULTIPLE_UNLOCK) {
          var removedIds = eventArgs.get_isMultipleEntities() ? $o.getEntityIDsFromEntities(eventArgs.get_entities()) : [eventArgs.get_entityId()];

          options = { removedIds: removedIds };
        }

        this.refreshGridGroup(entityTypeCode, parentEntityId, options);
      } else if (eventArgs.get_isUpdated()
        || eventArgs.get_isLoaded()
        || actionTypeCode == window.ACTION_TYPE_CODE_CANCEL
        || actionTypeCode == window.ACTION_TYPE_CODE_CHANGE_LOCK) {
        this.refreshGridGroupWithChecking(entityTypeCode, parentEntityId, entityId);
      }

      // additional refreshes
      if (eventArgs.get_isUpdated() && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE) {
        this.refreshGridGroup(window.ENTITY_TYPE_CODE_ARTICLE_VERSION, entityId);
      } else if ((eventArgs.get_isArchiving() || eventArgs.get_isRemoving()) && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE) {
        this.refreshGridGroup(window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE, parentEntityId);
      } else if (eventArgs.get_isRestoring() && entityTypeCode == window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE) {
        this.refreshGridGroup(window.ENTITY_TYPE_CODE_ARTICLE, parentEntityId);
      } else if (eventArgs.get_isRestored() && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
        parentEntityId = +$o.getParentEntityId(window.ENTITY_TYPE_CODE_ARTICLE, entityId) || 0;
        this.refreshGridGroup(window.ENTITY_TYPE_CODE_ARTICLE, parentEntityId);
      } else if (actionTypeCode == window.ACTION_TYPE_CHILD_ENTITY_PERMISSION_SAVE
        || actionTypeCode == window.ACTION_TYPE_CHILD_ENTITY_MULTIPLE_REMOVE
        || actionTypeCode == window.ACTION_TYPE_CHILD_ENTITY_REMOVE_ALL
        || actionTypeCode == window.ACTION_TYPE_CHILD_ENTITY_REMOVE
      ) {
        if (entityTypeCode == window.ENTITY_TYPE_CODE_CONTENT_PERMISSION) {
          this.refreshGridGroup(window.ACTION_CODE_CHILD_CONTENT_PERMISSIONS, parentEntityId);
          this.refreshGridGroup(window.ACTION_CODE_CONTENT_PERMISSIONS);
        } else if (entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE_PERMISSION) {
          this.refreshGridGroup(window.ACTION_CODE_CHILD_ARTICLE_PERMISSIONS, parentEntityId);
          this.refreshGridGroup(window.ACTION_CODE_ARTICLE_PERMISSIONS);
        }
      } else if (eventArgs.get_isSaved() || eventArgs.get_isUpdated() || eventArgs.get_isRemoving()) {
        if (entityTypeCode == window.ENTITY_TYPE_CODE_CONTENT_PERMISSION) {
          this.refreshGridGroup(window.ACTION_CODE_CHILD_CONTENT_PERMISSIONS);
        } else if (entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE_PERMISSION) {
          this.refreshGridGroup(window.ACTION_CODE_CHILD_ARTICLE_PERMISSIONS);
        }
      }
    }
  },

  dispose: function () {
    Quantumart.QP8.BackendEntityGridManager.callBaseMethod(this, 'dispose');

    if (this._gridGroups) {
      for (let gridGroupCode in this._gridGroups) {
        var gridGroup = this._gridGroups[gridGroupCode];
        Object.keys(gridGroup).forEach(this.destroyGrid);
      }
    }

    Quantumart.QP8.BackendEntityGridManager._instance = null;
    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendEntityGridManager._instance = null;

Quantumart.QP8.BackendEntityGridManager.getInstance = function () {
  if (Quantumart.QP8.BackendEntityGridManager._instance == null) {
    Quantumart.QP8.BackendEntityGridManager._instance = new Quantumart.QP8.BackendEntityGridManager();
  }

  return Quantumart.QP8.BackendEntityGridManager._instance;
};

Quantumart.QP8.BackendEntityGridManager.destroyInstance = function () {
  if (Quantumart.QP8.BackendEntityGridManager._instance) {
    Quantumart.QP8.BackendEntityGridManager._instance.dispose();
  }
};

Quantumart.QP8.BackendEntityGridManager.registerClass('Quantumart.QP8.BackendEntityGridManager', Quantumart.QP8.Observable);

