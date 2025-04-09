function getCookieValue(name) {
  const cookies = document.cookie;
  const regex = new RegExp('(^|; )' + name + '=([^;]*)');
  const match = regex.exec(cookies);

  if (match) {
    return decodeURIComponent(match[2]);
  } else {
    return null;
  }
}

export default getCookieValue;
