import { BackendArticleSearchBlock } from '../Search/BackendArticleSearchBlock';
import { BackendContentSearchBlock } from '../Search/BackendContentSearchBlock';
import { BackendContextBlock } from '../Search/BackendContextBlock';
import { BackendSearchBlockBase } from '../Search/BackendSearchBlockBase';
import { BackendUserSearchBlock } from '../Search/BackendUserSearchBlock';
import { Observable } from '../Common/Observable';
import { $q } from '../Utils';

export class BackendSearchBlockManager extends Observable {
  /** @type {BackendSearchBlockManager} */
  static _instance;

  static getInstance() {
    if (!BackendSearchBlockManager._instance) {
      BackendSearchBlockManager._instance = new BackendSearchBlockManager();
    }

    return BackendSearchBlockManager._instance;
  }

  static destroyInstance() {
    if (BackendSearchBlockManager._instance) {
      BackendSearchBlockManager._instance.dispose();
      BackendSearchBlockManager._instance = null;
    }
  }

  static generateSearchBlockGroupCode(entityTypeCode, parentEntityId) {
    return `${entityTypeCode}_${parentEntityId}`;
  }

  constructor() {
    super();
    this._searchBlockGroups = {};
  }

  getSearchBlockGroup(searchBlockGroupCode) {
    return this._searchBlockGroups[searchBlockGroupCode];
  }

  createSearchBlockGroup(searchBlockGroupCode) {
    this._searchBlockGroups[searchBlockGroupCode] = this.getSearchBlockGroup(searchBlockGroupCode) || {};
    return this._searchBlockGroups[searchBlockGroupCode];
  }

  removeSearchBlockGroup(searchBlockGroupCode) {
    $q.removeProperty(this._searchBlockGroups, searchBlockGroupCode);
  }

  getSearchBlock(searchBlockElementId) {
    const searchBlockGroup = Object.values(this._searchBlockGroups).find(val => val[searchBlockElementId]);
    return searchBlockGroup[searchBlockElementId];
  }

  createSearchBlock(searchBlockElementId, entityTypeCode, parentEntityId, host, options) {
    const searchBlockGroupCode = BackendSearchBlockManager.generateSearchBlockGroupCode(entityTypeCode, parentEntityId);

    let searchBlock = null;
    if (options.contextSearch) {
      searchBlock = new BackendContextBlock(
        searchBlockGroupCode,
        searchBlockElementId,
        entityTypeCode,
        parentEntityId,
        options
      );
    } else if (entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE
      || entityTypeCode === window.ENTITY_TYPE_CODE_VIRTUAL_ARTICLE
      || entityTypeCode === window.ENTITY_TYPE_CODE_ARCHIVE_ARTICLE
    ) {
      if (host
        && host.get_documentContext()
        && host.get_documentContext().getOptions()
        && host.get_documentContext().getOptions().isVirtual
      ) {
        Object.assign(options, { isVirtual: true });
      }

      searchBlock = new BackendArticleSearchBlock(
        searchBlockGroupCode,
        searchBlockElementId,
        entityTypeCode,
        parentEntityId,
        options
      );
    } else if (entityTypeCode === window.ENTITY_TYPE_CODE_CONTENT) {
      searchBlock = new BackendContentSearchBlock(
        searchBlockGroupCode,
        searchBlockElementId,
        entityTypeCode,
        parentEntityId,
        options
      );
    } else if (entityTypeCode === window.ENTITY_TYPE_CODE_USER) {
      searchBlock = new BackendUserSearchBlock(
        searchBlockGroupCode,
        searchBlockElementId,
        entityTypeCode,
        parentEntityId,
        options
      );
    } else {
      searchBlock = new BackendSearchBlockBase(
        searchBlockGroupCode,
        searchBlockElementId,
        entityTypeCode,
        parentEntityId,
        options
      );
    }

    searchBlock.initialize();

    const searchBlockGroup = this.createSearchBlockGroup(searchBlockGroupCode);
    searchBlockGroup[searchBlockElementId] = searchBlock;

    return searchBlock;
  }

  removeSearchBlock(searchBlockElementId) {
    const searchBlock = this.getSearchBlock(searchBlockElementId);
    if (searchBlock) {
      const searchBlockGroupCode = searchBlock.get_searchBlockGroupCode();
      const searchBlockGroup = this.getSearchBlockGroup(searchBlockGroupCode);
      $q.removeProperty(searchBlockGroup, searchBlockElementId);
      if ($q.getHashKeysCount(searchBlockGroup) === 0) {
        this.removeSearchBlockGroup(searchBlockGroupCode);
      }
    }
  }

  destroySearchBlock(searchBlockElementId) {
    const searchBlock = this.getSearchBlock(searchBlockElementId);
    if (searchBlock) {
      this.removeSearchBlock(searchBlockElementId);
      if (searchBlock.dispose) {
        searchBlock.dispose();
      }
    }
  }

  dispose() {
    super.dispose();
    if (this._searchBlockGroups) {
      Object.values(this._searchBlockGroups).forEach(searchBlockGroup => {
        Object.keys(searchBlockGroup).forEach(this.destroySearchBlock, this);
      }, this);
    }

    this._searchBlockGroups = null;
    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendSearchBlockManager = BackendSearchBlockManager;
