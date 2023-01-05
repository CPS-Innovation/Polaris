export const getFileNameWithoutExtension = (filename: undefined | string) => {
  if (filename === undefined) {
    return "";
  }
  return filename.substring(0, filename.lastIndexOf(".")) || filename;
};
