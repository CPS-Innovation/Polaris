const twoLevelStringify = (storage: Storage) => {
  var result = { ...storage };

  for (var key in result) {
    try {
      // if a session value is a JSON string, we hydrate that string to an object ...
      result[key] = JSON.parse(result[key]);
    } catch (_) {
      // ... otherwise it gets left alone if it is not a JSON object
    }
  }
  // ... and our output is nicely formatted JSON representation with hydrated values, where found.
  return JSON.stringify(result, null, 2);
};

export const Auth: React.FC = () => {
  return (
    <>
      <h1>Client Id: {process.env.REACT_APP_CLIENT_ID}</h1>
      <div style={{ margin: 10 }}>
        <h2>Local Storage</h2>
        <code>
          <pre>{twoLevelStringify(window.localStorage)}</pre>
        </code>
        <h2>Session Storage</h2>
        <code>
          <pre>{twoLevelStringify(window.sessionStorage)}</pre>
        </code>
        <p>
          <a href="/">Home</a>
        </p>
      </div>
    </>
  );
};
