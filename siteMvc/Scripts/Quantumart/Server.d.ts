interface TreeNode {
  Id: number;
  Code: string;
  ParentId: number;
  ParentGroupId?: any;
  IsFolder: boolean;
  IsGroup: boolean;
  GroupItemCode?: any;
  Icon: string;
  Title: string;
  DefaultActionCode: string;
  DefaultActionTypeCode: string;
  ContextMenuCode: string;
  HasChildren: boolean;
  ChildNodes?: TreeNode[];
}
