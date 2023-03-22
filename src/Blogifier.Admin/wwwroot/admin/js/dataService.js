let DataService = function () {
  let upload = function (url, obj, success, fail) {
    const TenMinutes = 600000;
    axios
      .post(url, obj, { 
        timeout: TenMinutes,
        maxContentLength: 6291456,
        maxBodyLength: 6291456,
        onUploadProgress: (progressEvent) => {
          console.log(progressEvent.loaded + ' ' + progressEvent.total);
        }
      })
      .then(response => response)
      .then(data => success(data))
      .catch(error => fail(error));
  };
  return {
    upload: upload
  };
}();
