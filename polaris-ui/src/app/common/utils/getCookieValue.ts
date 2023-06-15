export const getCookieValue = (cookieName: string): string | null => {
  const cookies: string[] = document.cookie.split(";");

  for (let i = 0; i < cookies.length; i++) {
    const cookie: string = cookies[i].trim();

    if (cookie.startsWith(`${cookieName}=`)) {
      return cookie.substring(cookieName.length + 1);
    }
  }

  return null;
};
