export type PageRotation = {
  pageNumber: number;
  rotationAngle: number;
};
export interface IPageRotation extends PageRotation {
  id: string;
}

export type RotationSaveRequest = {
  pageIndex: number;
  operation: "rotate";
  arg: string;
}[];
