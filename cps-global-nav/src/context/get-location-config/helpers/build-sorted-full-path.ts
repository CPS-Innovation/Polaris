export const buildSortedFullPath = ({ pathname, search }: Location) => {
  const urlParams = new URLSearchParams(search);
  urlParams.sort();
  return pathname + (urlParams.size ? "?" : "") + urlParams.toString();
};
