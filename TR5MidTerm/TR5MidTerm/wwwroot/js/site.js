// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/// 更新連動選單
/// targetId : 要更新的選單id, 例 '#部門'
/// $targetComponent : 要更新的選單JQuery物件, 若同時和targetId使用, 以targetId優先
/// api : 對應的api, 例:'@Url.Action("取得部門選單", "Menu")'
/// data : 呼叫api的參數, 例:{ "單位": 目前單位 }
/// type : get(default) or post
/// cahce : false(default)
/// aysnc : true(default)
/// dataType : json(default) or text
/// defaultValue : 選單載入後的預設選取值
/// processData($ddlTarget, data, defaultValue) : 如何處理資料, 若沒有則當資料是List<SelectListItem>去處理
/// success($ddlTarget) : ajax執行成功且更新選單後呼叫的額外函式, 若沒有則不執行
/// error($ddlTarget, xhr, status, throwError) : ajax執行失敗時呼叫的額外函式, 若沒有則不執行
/// 例:
///$("#FA列帳事業").change(function () {
///    var 目前事業 = $(this).val();
///    UpdateCasadeMenu(
///        {
///            targetId: '#FA列帳單位',
///            api: '@Url.Action("依事業取得單位選單", "Menu")',
///            data: { "事業": 目前事業 },
///            success: (_ => $("#FA列帳單位").change())
///        }
///    );
///});

/// 1140110:purify.js is necessary

function UpdateCasadeMenu(setting, callback) {
    setting = {
        type: 'get',
        cache: false,
        dataType: 'json',
        aysnc: true,
        ...setting // destructuring
    };

    let $ddlTarget;
    if (!setting.targetId) {
        $ddlTarget = setting.$targetComponent;
    }
    else {
        $ddlTarget = $(setting.targetId);
    }
    $ddlTarget.html('');
    $.ajax({
        url: setting.api,
        data: setting.data,
        type: setting.type,
        aysnc: setting.aysnc,
        cache: setting.cache,
        dataType: setting.dataType,
        success: function (data) {
            if (setting.processData != undefined) {
                setting.processData($ddlTarget, data, setting.defaultValue);
            }
            else {
                $.each(data, function (i, item) {
                    let $newEle = $('<option></option>').val(item.Value).text(item.Text);
                    if (item.Value == setting.defaultValue) {
                        $newEle.attr('selected', 'selected');
                    }
                    $ddlTarget.append(DOMPurify.sanitize($newEle[0]));
                });
            }
            if (setting.success != undefined) {
                setting.success($ddlTarget);
            }

            if (typeof callback === 'function') {
                callback();
            }
        },
        error: function (xhr, status, thrownError) {
            console.log(status + "/" + thrownError);
            if (setting.error != undefined) {
                setting.error($ddlTarget, xhr, status, thrownError);
            }
        }
    });
}

/// Async版的UpdateCasadeMenu
/// 不可設定async(固定為true)
/// 不可設定success($ddlTarget) callback, 請以.then($ddlTarget => ...)處理 或 await處理
/// 不可設定error($ddlTarget, xhr, status, throwError) callback, 請以.catch(({$ddlTarget, xhr, status, throwError}) => ...)處理 或 catch處理
/// 例1 Promise:
///$("#FA列帳事業").change(function () {
///    var 目前事業 = $(this).val();
///    UpdateCasadeMenu(
///        {
///            targetId: '#FA列帳單位',
///            api: '@Url.Action("依事業取得單位選單", "Menu")',
///            data: { "事業": 目前事業 },
///        }
///    ).then(_ => $("#FA列帳單位").change());
///});
/// 例2 Async/Await:
///$("#FA列帳事業").change(async function () {
///    var 目前事業 = $(this).val();
///    try {
///        let result = await UpdateCasadeMenuAsync(
///            {
///                targetId: '#FA列帳單位',
///                api: '@Url.Action("依事業取得單位選單", "Menu")',
///                data: { "事業": 目前事業 },
///            }
///        );
///        $("#FA列帳單位").change();
///    }
///    catch (err) {
///        console.log(err);
///    }
//});
async function UpdateCasadeMenuAsync(setting) {
    setting = {
        type: 'get',
        cache: false,
        dataType: 'json',
        ...setting // destructuring
    };
    let $ddlTarget;
    if (!setting.targetId) {
        $ddlTarget = setting.$targetComponent;
    }
    else {
        $ddlTarget = $(setting.targetId);
    }
    $ddlTarget.html('');

    try {
        let response = await $.ajax({
            url: setting.api,
            data: setting.data,
            type: setting.type,
            aysnc: true,
            cache: setting.cache,
            dataType: setting.dataType,
        });

        if (setting.processData != undefined) {
            setting.processData($ddlTarget, response, setting.defaultValue);
        }
        else {
            $.each(response, function (i, item) {
                let $newEle = $('<option></option>').val(item.Value).text(item.Text);
                if (item.Value == setting.defaultValue) {
                    $newEle.attr('selected', 'selected');
                }
                $ddlTarget.append(DOMPurify.sanitize($newEle[0]));
            });
        }

        return $ddlTarget;
    } catch (xhr) {
        console.log(xhr.status + "/" + xhr.statusText);
        throw {
            $ddlTarget: $ddlTarget,
            xhr: xhr,
            status: xhr.status,
            throwError: xhr.statusText
        };
    }
}

function UpdateDataListMenu(setting) {
    let dataList = $(setting.targetID);
    dataList.empty(); // 清空現有的選單

    axios({
        method: 'post',
        url: setting.api,
        headers: {
            'content-type': 'application/json',
            RequestVerificationToken: setting.token
        },
        // API要求的資料
        params: setting.data
    })
        .then(function (response) {
            $.each(response.data, function (index, item) {
                // 檢查屬性是否存在，然後加到 datalist 中
                if (item.Text !== undefined && item.Value !== undefined) {
                    dataList.append(`<option value="${item.Value}">${item.Text}</option>`);
                }
            });
        })
        .catch(function (error) {
            // 錯誤時的處理
        });
}


/// 顯示伺服器端的驗證結果
/// 1. 伺服器端請將ModelState放在data(注意大小寫)並以Ok回傳(目前前台底層僅能處理Ok)
///   return Ok(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR){ data = ModelState.ToErrorInfos()});
///   或是將訊息放在message(注意大小寫)
///   return Ok(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR){ message = "message to show"});
/// 2. 在對應頁面的form後的第一行增加一區塊供存放未對應欄位訊息
///    <div id="自訂" class="validation-summary-valid text-danger">
///        <ul>
///        </ul>
///    </div>
///    或是將asp-validation-summary改為true (這會導致前台驗證也出現在此區塊)
///    <div asp-validation-summary="All" class="text-danger"></div>
/// 3. 修改Index.cshtml的_serverReturned
///    const _serverReturned = function (retData) {
///    if (retData.returnCode != "OK") {
///        if (retData.message != undefined) {
///            ShowValidateErrorMessage(
///                retData.message,
///                {
///                    filter: '#div的id' /// asp-validation-summary=true時可省略
///                });
///        }
///        else {
///            ShowValidateErrors(
///                retData.data,
///                {
///                    filter: '#div的id' /// asp-validation-summary=true時可省略
///                });
///        }
///    }
///    else {
///        ClearValidateErrors({
///            filter: '#div的id' /// asp-validation-summary=true時可省略
///        });
///    }
///    }
///    4. 第3點也可以省略為
///       ShowValidateResult(retData, {filter: '#div的id'}) /// asp-validation-summary=true時可省略filter
///    5. 如果有多個form
///       ShowValidateResult(retData, {filter: '#div的id', $targetForm:$(#form_id)}) /// asp-validation-summary=true時可省略filter
///
/// 顯示後端ModelState錯誤至對應span上
/// data: 錯誤訊息的資料
/// setting.$targetForm : 對應的form, 省略時為$("form").last()
/// setting.filter : 錯誤訊息區的selector, 省略時為'[data-valmsg-summary=true]'
/// setting.OnlyShowMainDataResult : 新版面的一筆主檔多筆明細，請設定為true，Detail使用底層顯示錯誤訊息，因此不再使用ShowValidateErrors顯示Detail的Data錯誤訊息
function ShowValidateErrors(data, setting) {
    let errors = {};
    setting = {
        $targetForm: $("form").last(),
        filter: '[data-valmsg-summary=true]',
        ...setting // destructuring
    };

    let $summary = setting.$targetForm.find(setting.filter)
        .addClass("validation-summary-errors")
        .removeClass("validation-summary-valid");

    let $ul = $summary.find("ul").empty();

    let $validator = setting.$targetForm.validate();
    if (data) {

        if (setting.OnlyShowMainDataResult) {
            //Data錯誤訊息，只顯示Main
            for (let i = 0; i < data.length; i++) {
                let key = data[i].Key;
                let message = data[i].Value.join();
                if (key.includes("Main") && $validator.findByName(key).length > 0) {
                    errors[key] = message;
                }
                else if (!key.includes("Detail")) {
                    if ($ul.length > 0) {
                        $("<li />").html(message).appendTo($ul);
                    }
                }
            }
        } else {

            for (let i = 0; i < data.length; i++) {
                let key = data[i].Key;
                let message = data[i].Value.join();
                if (key && $validator.findByName(key).length > 0) {
                    errors[key] = message;
                }
                else {
                    if ($ul.length > 0) {
                        $("<li />").html(message).appendTo($ul);
                    }
                }
            }
        }

    }

    $validator.showErrors(errors);
}

/**
 * 清空JQuery驗證錯誤
 * setting.$targetForm : 對應的form, 省略時為$("form").last()
 * setting.filter : 錯誤訊息區的selector, 省略時為'[data-valmsg-summary=true]'
 * spotlightSelector: array, 僅清除該array內的元素，其餘不處理，如[#some-id, .some-class]。
 */
function ClearValidateErrors(setting) {
    setting = {
        $targetForm: $("form").last(),
        filter: '[data-valmsg-summary=true]',
        spotlightSelector: [],
        ...setting // destructuring
    };

    const $form = setting.$targetForm;

    let $summary = $form.find(setting.filter)
        .addClass("validation-summary-valid")
        .removeClass("validation-summary-errors");

    let $ul = $summary.find("ul").empty();

    let $validator = $form.validate();

    // get errors that were created using jQuery.validate.unobtrusive
    let $errors = null;

    if (setting.spotlightSelector &&
        Array.isArray(setting.spotlightSelector) &&
        setting.spotlightSelector.length > 0) {
        $errors = $form.find(setting.spotlightSelector.join(',')).
            find(".field-validation-error span");
    }
    else {
        $errors = $form.find(".field-validation-error span");
    }

    // trick unobtrusive to think the elements were succesfully validated
    // this removes the validation messages
    $errors.each(function () { $validator.settings.success($(this)); })

    // clear errors from validation
    $validator.resetForm();
}

/// 顯示錯誤訊息
/// message : 錯誤訊息
/// setting.filter : 錯誤訊息區的selector, 省略時為'[data-valmsg-summary=true]'
function ShowValidateErrorMessage(message, setting) {
    setting = {
        $targetForm: $("form").last(),
        filter: '[data-valmsg-summary=true]',
        ...setting // destructuring
    };

    let $summary = setting.$targetForm.find(setting.filter)
        .addClass("validation-summary-errors")
        .removeClass("validation-summary-valid");

    let $ul = $summary.find("ul").empty();

    if ($ul.length != 0) {
        $("<li />").html(message).appendTo($ul);
    }
}

/// 顯示後端ModelState錯誤
/// retData: 後端retData類別產生之物件
/// setting.$targetForm : 對應的form, 省略時為$("form").last()
/// setting.filter : 錯誤訊息區的selector, 省略時為'[data-valmsg-summary=true]'
function ShowValidateResult(retData, setting) {
    setting = {
        $targetForm: $("form").last(),
        filter: '[data-valmsg-summary=true]',
        ...setting
    };

    /// 為避免頁面有驗證訊息殘留
    /// 因此收到底層的驗證結果不管對錯都要清除驗證訊息
    ClearValidateErrors(
        {
            $targetForm: setting.$targetForm,
            filter: setting.filter
        },
    );

    if (retData.returnCode !== "OK") {
        if (retData.message) {
            ShowValidateErrorMessage(
                retData.message,
                {
                    $targetForm: setting.$targetForm,
                    filter: setting.filter,
                }
            );
        }
        else {
            ShowValidateErrors(
                retData.data,
                {
                    $targetForm: setting.$targetForm,
                    filter: setting.filter,
                    OnlyShowMainDataResult: setting.OnlyShowMainDataResult
                }
            );
        }
    }
}

/// 將js的日期化為yyyy-MM-dd字串
function FormatDate(date, separator = 'slash') {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    switch (separator) {
        case 'slash':
            return `${year}/${month}/${day}`;
        case 'dash':
            return `${year}-${month}-${day}`;
        default:
            console.log('FormatDate: invalid separator');
            debugger;
            return;
    }
}

function FormatDateAsDateOnly(dateVal) {
    // Handle null: moment(null) results in an invalid moment object.
    if (dateVal === null) {
        return "Invalid date";
    }

    // Handle undefined: moment(undefined) uses the current time.
    // new Date() with no arguments also uses current time.
    // new Date(undefined) results in an "Invalid Date". So, explicit handling for undefined.
    const d = (dateVal === undefined) ? new Date() : new Date(dateVal);

    // Check if d is an "Invalid Date" (e.g., from new Date("invalid string") or new Date(undefined) if not handled above)
    if (isNaN(d.getTime())) {
        return "Invalid date"; // Consistent with moment's output for unparseable strings.
    }

    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0'); // getMonth() is 0-indexed
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}/${month}/${day}`;
}

function FormatDateAsTimeOnly(dateVal) {
    if (dateVal === null) {
        return "Invalid date";
    }
    const d = (dateVal === undefined) ? new Date() : new Date(dateVal);

    if (isNaN(d.getTime())) {
        return "Invalid date";
    }

    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    const seconds = String(d.getSeconds()).padStart(2, '0');
    return `${hours}:${minutes}:${seconds}`;
}

function FormatDateAsDateTime(dateVal) {
    if (dateVal === null) {
        return "Invalid date";
    }
    const d = (dateVal === undefined) ? new Date() : new Date(dateVal);

    if (isNaN(d.getTime())) {
        return "Invalid date";
    }

    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    const seconds = String(d.getSeconds()).padStart(2, '0');
    return `${year}/${month}/${day} ${hours}:${minutes}:${seconds}`;
}

function FormatDateByDataType(dateVal, dataType) {
    if (dataType == 2 /*Date*/ || dataType == 'date') {
        return FormatDateAsDateOnly(dateVal);
    }
    else if (dataType == 3 /*Time*/ || dataType == 'time') {
        return FormatDateAsTimeOnly(dateVal);
    }
    else {
        return FormatDateAsDateTime(dateVal);
    }
}

/// JSON需要ISO8601格式, 所以送往伺服器前先進行處理
function FormatDateStringAsISO8601(dateVal) {
    if (typeof (dateVal) == 'string') {
        /// 已經是ISO8601了
        let iso8601Date = moment(dateVal, moment.ISO_8601, true)
        if (iso8601Date.isValid()) {
            return iso8601Date;
        }
    }

    return moment(dateVal).toISOString(true);
}

function isScrollable(ele) {
    const hasScrollableContent = ele.scrollHeight > ele.clientHeight;

    const overflowYStyle = window.getComputedStyle(ele).overflowY;
    const isOverflowHidden = overflowYStyle.indexOf('hidden') !== -1;

    return hasScrollableContent && !isOverflowHidden;
};

/// 找出最接近的可捲動元件
/// ele只接受htmlElemet(document.querySelector(...)), 請不要把JQuery結果直接丟進來
function getScrollableParent(ele) {
    return !ele || ele === document.body
        ? document.body
        : isScrollable(ele)
            ? ele
            : getScrollableParent(ele.parentNode);
};

/// 在不使用JQuery驗證時, 清空驗證錯誤
/// setting.$targetForm : 對應的form, 省略時為$("form").last()
/// setting.filter : 錯誤訊息區的selector, 省略時為'[data-valmsg-summary=true]'
function ClearValidateErrorsWithoutJQueryValidate(setting) {
    setting = {
        $targetForm: $("form").last(),
        filter: '[data-valmsg-summary=true]',
        ...setting // destructuring
    };

    let $summary = setting.$targetForm.find(setting.filter)
        .addClass("validation-summary-valid")
        .removeClass("validation-summary-errors");

    let $ul = $summary.find("ul").empty();

    setting.$targetForm.find("span[data-valmsg-for]").text('');
}


function ConvertErrorMessage(data) {
    let ulContent = '<ul style="list-style-type: none; padding: 0; margin: 0;">';

    for (let i = 0; i < data.length; i++) {
        let key = data[i].Key;
        let message = data[i].Value.join(', ');
        ulContent += `<li>${key}：${message}</li>`;
    }

    ulContent += '</ul>';
    return ulContent;
}

/// 在不使用JQuery驗證時, 顯示後端ModelState錯誤
/// retData: 後端retData類別產生之物件
/// setting.$targetForm : 對應的form, 省略時為$("form").last()
/// setting.filter : 錯誤訊息區的selector, 省略時為'[data-valmsg-summary=true]'
/// setting.prefix : 錯誤訊息的鍵值前置
function ShowValidateResultWithoutJQueryValidate(retData, setting) {
    setting = {
        $targetForm: $("form").last(),
        filter: '[data-valmsg-summary=true]',
        prefix: '',
        ...setting
    };

    /// 為避免頁面有驗證訊息殘留
    /// 因此收到底層的驗證結果不管對錯都要清除驗證訊息
    ClearValidateErrorsWithoutJQueryValidate(
        {
            $targetForm: setting.$targetForm,
            filter: setting.filter,
            prefix: setting.prefix
        },
    );

    if (retData == undefined) {
        ShowValidateErrorMessage(
            '執行異常',
            {
                $targetForm: setting.$targetForm,
                filter: setting.filter,
            });
        return;
    }

    if (retData.message != undefined) {
        ShowValidateErrorMessage(
            retData.message,
            {
                $targetForm: setting.$targetForm,
                filter: setting.filter,
            });
    }

    if (retData.data !== undefined) {
        retData.data.forEach(function (element) {
            let key = element.Key;
            let message = element.Value.join();
            setting.$targetForm.find("span[data-valmsg-for='" + setting.prefix + key + "']").text(message);

        });
    }
}

/**
 * 將底層包在FileResultDescription取得的資料轉成下載
 * @param {object} retData - server returned file object
 * @param {boolean} isPrintWithoutOpen - prints file content directly.
 */
function DownloadFileFromFileResultDescription(retData, isPrintWithoutOpen = false) {
    if (retData.returnCode != "OK" || retData?.data?.Content == undefined && retData?.data?.FileName == undefined) {
        return;
    }

    DownloadFileFromResponse(retData?.data?.Content, retData?.data?.FileName, null, isPrintWithoutOpen, retData?.data?.ExportedCount);
}

/// 將blob(由FileResult回傳)或base64string(由json回傳)轉成下載
function DownloadFileFromResponse(content, fileName, contentType, isPrintWithoutOpen = false, exportedCount) {
    fileName = fileName ?? '檔名未定義';
    contentType = contentType ?? 'application/octet-stream';
    let isPdf = fileName.endsWith('.pdf')
    if (isPdf) {
        contentType = 'application/pdf'
    }
    /// 如果伺服器是回傳FileResult, 會是blob
    /// 如果伺服器是回傳Json, byte[]轉成json會變成base64
    let href = null;
    let isBlob = true;
    if (typeof (content) == 'string') {
        href = `data:${contentType};base64,${DOMPurify.sanitize(content)}`;
        isBlob = false;
    }
    else if (content instanceof Blob) {
        /// Blob本身已有type
        href = URL.createObjectURL(content);
    }
    else {
        /// binary array
        href = URL.createObjectURL(new Blob([content], {
            type: contentType
        }));
    }
    if (isPrintWithoutOpen || isPdf) {

        //base64 to blob
        if (isBlob === false) {

            const byteCharacters = atob(content);
            //將base64格式的字串中的字元逐一存入陣列
            const byteArrays = Array.from(byteCharacters, char => char.charCodeAt(0));

            const byteArray = new Uint8Array(byteArrays);
            href = URL.createObjectURL(new Blob([byteArray], { type: contentType }));
        }

        if (isPdf) {
            const newWindow = window.open(href, '_blank', "width=800,height=600"); // 在新視窗或分頁打開 PDF
            newWindow.document.write(`
<html>
  <head>
    <title>${fileName}</title>
 <style>
        body {
            margin: 0;
            padding: 0;
            height: 100vh;
            width: 100vw;
            display: flex;
            flex-direction: column;
        }

      #downloadBtn {
            position: absolute;
            top: 13px;    /* 距離頁面頂部的距離 */
            right: 90px;  /* 靠右對齊 */
            padding: 5px 10px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-size: 14px;
        }

        iframe {
            flex: 1;
            width: 100%;
            border: none;
        }
    </style>
  </head>
  <body>
    <button id="downloadBtn">下載檔案</button>
    <iframe id="fileFrame" src="${DOMPurify.sanitize(href)}" width="100%" height="100%"></iframe>
    <script>
      document.getElementById("downloadBtn").addEventListener("click", () => {
        const link = document.createElement("a");
        link.href = document.getElementById("fileFrame").src;
        link.download = "${fileName}";  // 自訂下載檔名
        link.click();
      });
    </script>
  </body>
</html>`);

            return;
        }
        //loading file content into an iframe for printing.
        const hiddenFrame = document.createElement('iframe');
        hiddenFrame.style.display = "none";
        hiddenFrame.src = href;
        hiddenFrame.onload = function () {
            const closePrint = () => {
                document.body.removeChild(this);
            };
            this.contentWindow.onbeforeunload = closePrint;
            this.contentWindow.onafterprint = closePrint;
            this.contentWindow.print();
        };

        document.body.appendChild(hiddenFrame);
        return;
    }

    // create "a" HTML element with href to file & click to download
    const link = document.createElement('a');
    link.href = href;
    link.setAttribute('download', fileName); //or any other extension
    document.body.appendChild(link);
    link.click();

    // clean up "a" element & remove ObjectURL
    document.body.removeChild(link);
    URL.revokeObjectURL(href);

    if (exportedCount != null) {
        alert(`共匯出${exportedCount}筆資料`);
    }
}

/// 處理按鈕直接下載(回傳FileResult), 並且將錯誤(以其他StatusCode回傳TscLibCore.Commons.ReturnData)顯示在頁面上
/// url : 下載連結
/// options.params : 下載參數, 預設為undefined
/// options.method : 下載方式, 預設為get
/// options.defaultFileName : 當content-disposition未指定檔名時, 會以此作為檔名
/// options.alertTime : 錯誤資訊顯示時間, 預設為0sec(小於等於0時:永久)
async function downloadFileViaAxiosAndAlertForFail(url, options) {
    try {
        options = {
            method: 'get',
            alertTime: 0,
            ...options // destructuring
        };

        let response = await axios.options(url,
            {
                method: options.method,
                params: options.params,
                responseType: 'blob'
            });

        let fileName = undefined;
        var disposition = response.headers['content-disposition'];
        if (disposition && disposition.indexOf('attachment') !== -1) {
            var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
            var matches = filenameRegex.exec(disposition);
            if (matches != null && matches[1]) {
                fileName = decodeURI(matches[1].replace(/['"]/g, ''));
            }
        }

        if (fileName == undefined) {
            fileName = options.defaultFileName;
        }

        DownloadFileFromResponse(response.data, fileName, response.headers['content-type'])
    }
    catch (err) {
        console.log(err);

        if (err.response.data instanceof Blob) {
            const blob = new Blob([err.response.data]);
            const data = await blob.text();
            const tempReturnData = JSON.parse(data)
            if (tempReturnData.message != undefined) {
                if (options.alertTime <= 0) {
                    SharedVue.alertBox().noCountDown().showErrorAlert("下載失敗", tempReturnData.message);
                }
                else {
                    SharedVue.alertBox(options.alertTime).showErrorAlert("下載失敗", tempReturnData.message);
                }
            }
            else if (tempReturnData.data != undefined) {
                var msgContent = '<ul>';
                tempReturnData.data.forEach(function (element) {
                    msgContent += `<li>${element.Value}</li>`;
                });
                msgContent += '</ul>';
                if (options.alertTime <= 0) {
                    SharedVue.alertBox().noCountDown().showErrorAlert("下載失敗", msgContent);
                }
                else {
                    SharedVue.alertBox(options.alertTime).showErrorAlert("下載失敗", msgContent);
                }
            }
            else {
                if (options.alertTime <= 0) {
                    SharedVue.alertBox().noCountDown().showErrorAlert("下載失敗", '執行發生異常');
                }
                else {
                    SharedVue.alertBox(options.alertTime).showErrorAlert("下載失敗", '執行發生異常');
                }
            }
        }
        else {
            if (options.alertTime <= 0) {
                SharedVue.alertBox().noCountDown().showErrorAlert("下載失敗", '執行發生異常');
            }
            else {
                SharedVue.alertBox(options.alertTime).showErrorAlert("下載失敗", '執行發生異常');
            }
        }
    }
}

/// 後台以List<SelecteListIitem>回傳到前台時, 與BootStrapVue的Select所需資料的轉換
function ConvertSelectListItemsToBFormSelectOptions(listItems) {
    /// lambda直接回
    return listItems.map(listItem =>
    (
        {
            value: listItem.Value,
            text: listItem.Text,
            disabled: listItem.Disabled
        }
    ));
}

/// 產生TempusDominus的設定(24小時制+中文化)
/// setting.stepping : 1(default), 選單滑動增減分鐘數
/// setting.allowInputToggle: false(default), 使用者Focus輸入元素時, 彈出視窗
function CreateTimeOnlyTempusDominusOption(setting) {
    return {
        allowInputToggle: setting.allowInputToggle,
        useCurrent: false,
        stepping: setting.stepping, /// 每次增減時間的間隔
        display: {
            viewMode: 'clock',
            buttons: {
                close: true,
                clear: true,
            },
            /// 選擇視窗可選元件
            components: {
                decades: false,
                year: false,
                month: false,
                date: false,
                hours: true,
                minutes: true,
                seconds: false
            }
        },
        localization: {
            hourCycle: 'h23', /// 0-23:59
            format: 'HH:mm' /// 選定時間後的回傳值
        }
    };
}

/// 讓該文字輸入欄位, 點擊時會出現時間選擇視窗
/// html5的時間輸入會隨著瀏覽器改變, 所以用tempsDominus
///
/// element: html元素
/// setting.stepping : 1(default), 選單滑動增減分鐘數
/// setting.allowInputToggle: false(default), 使用者Focus輸入元素時, 彈出視窗
///
/// 回傳:時間元件
///
/// 例1:讓文字輸入觸發時間輸入
/// <input id="可收貨起始時分"
///    type="text"
///    asp-for="可收貨起始時分"
///    class="form-control"
///    v-maska data-maska="##:##"
///    placeholder="HH:mm"
/// />
/// <script>
/// ....
/// function OnReady() {
///     SetupTimeOnlyInput(document.getElementById('可收貨起始時分'))
/// }
/// </script>
/// 如果要該輸入欄位不讓使用者輸入, 記得設為readonly
///
/// 方法二:作為按鈕
/// /// data-td-target-input="nearest":選擇第一個input
/// /// data-td-target-toggle="nearest":選擇[data-td-toggle="datetimepicker"]
/// <div class="input-group"
///     id="可收貨終止時分時間選擇"
///     data-td-target-input="nearest" 
///     data-td-target-toggle="nearest"
///     >
///     <input id="可收貨終止時分"
///         type="text"
///         asp-for="可收貨終止時分"
///         class="form-control"
///         v-maska data-maska="##:##"
///         placeholder="HH:mm"
///     />
///     <span class="input-group-text"
///         data-td-toggle="datetimepicker"
///     >
///         <i class="fas fa-clock"></i>
///     </span>
/// </div>
///
/// <script>
/// ....
/// function OnReady() {
///     SetupTimeOnlyInput(document.getElementById('可收貨終止時分時間選擇'))
/// }
/// </script>
/// 註:
/// b-form提供的時間輸入並沒有內含input而是使用v-model綁定資料, 所以不適用底層架構的form submit
/// 如果需要使用b-form需要搭配自定變數
/// 另外b-form timepicker產生的值是hh:mm:ss, 若單純以字串驗證時要注意
///
//////////////////////////////////////////////////////////////////////
/// 方法1: 只允許從b-form-timepicker輸入搭配hidden input(unobstrusive預設不會驗證hidden, 要自行處理)
//////////////////////////////////////////////////////////////////////
/// <b-form-timepicker v-model="customVariable.可收貨起始時分"
///   v-bind="customVariable.pickerLabelOptions" :locale="'zh'"></b-form-timepicker>
/// <input type="hidden" asp-for="可收貨起始時分" v-model="customVariable.可收貨起始時分" class="force-validate" />
///
/// <script>
///  /// 因底層載入順序, 要在script最外層
///  let pickerLabelOptions = CreateBFormTimePickerZHTWLabelOptions();
///  Vue.set(this.customVariable, 'pickerLabelOptions', pickerLabelOptions);
///  Vue.set(this.customVariable, '可收貨起始時分', @Model.可收貨起始時分);
///  this
///    .$off('fetched-page-modal::ready')
///    .$on('fetched-page-modal::ready', () => { OnReady(); });
///
/// function OnReady() {
///    /// validate hidden field with class force-validate
///    var $form = $("form").last();
///    var validator = $form.validate();
///    validator.settings.ignore = ":hidden:not('.force-validate')";
/// }
/// </script>
//////////////////////////////////////////////////////////////////////
///  方法2: b-form-timepicker作為按鈕, input加上mask後會自動過濾掉秒數
//////////////////////////////////////////////////////////////////////
/// <b-input-group>
///	    <input type="text" asp-for="可收貨起始時分"
///             v-model="customVariable.可收貨起始時分"
///             class="form-control" v-maska data-maska="##:##" placeholder="HH:mm" />
///	    <b-input-group-append>
///		    <b-form-timepicker v-model="customVariable.可收貨起始時分"
///						   button-only
///						   v-bind="customVariable.pickerLabelOptions"
///						   :locale="'zh'"></b-form-timepicker>
///	    </b-input-group-append>
/// </b-input-group> 
//////////////////////////////////////////////////////////////////////
function SetupTimeOnlyInput(element, setting) {
    setting = {
        allowInputToggle: true,
        stepping: 1,
        ...setting // destructuring
    };

    let options = CreateTimeOnlyTempusDominusOption(setting);

    let format = options.localization.format || "HH:mm";

    let picker = new tempusDominus.TempusDominus(element, options);

    /// 使用moment去parse
    picker.dates.setFromInput = function (value, index) {
        const converted = moment(value, format);
        if (converted.isValid()) {
            const date = tempusDominus.DateTime.convert(converted.toDate(), this.optionsStore.options.localization.locale);
            this.setValue(date, index);
        }
        else {
            console.warn('Momentjs failed to parse the input date.');
        }
    };
    picker.dates.formatInput = function (date) {
        /// moment會把undefined轉成目前時間
        if (date == undefined) {
            return "";
        }
        return moment(date).format(format);
    };

    return picker;
}

/// 產生BFormTimePicker的中文化標籤
function CreateBFormTimePickerZHTWLabelOptions() {
    let pickerLabelOptions = {
        labelHours: '小時',
        labelMinutes: '分鐘',
        labelSeconds: '秒',
        labelAmpm: '上午下午',
        labelAm: '上午',
        labelPm: '下午',
        labelIncrement: '增加',
        labelDecrement: '減少',
        labelSelected: '選定時間',
        labelNoTimeSelected: '没有選擇時間',
        labelCloseButton: '關'
    }

    return pickerLabelOptions;
}

//@Html.Raw()的替代方案
//請傳入'@JsonSerializer.Serialize(List<SelectListItem>)'
//ex. parseAndConvertSelectListItems('@JsonSerializer.Serialize(ViewBag.差勤單位選單選項)')
function parseAndConvertSelectListItems(encodedJsonStr) {
    if (encodedJsonStr === null || encodedJsonStr === '') {
        return null;
    }

    return ConvertSelectListItemsToBFormSelectOptions(deserializeHtmlEncodedJsonObj(encodedJsonStr));
}

/// 用來處理Controller傳來的，先序列化為Json字串再HtmlEncode的物件［也就是HttpUtility.HtmlEncode(JsonSerializer.Serialize(some object))］
function deserializeHtmlEncodedJsonObj(
    encodedJsonStr,
    options = {
        replaceLessThanSign: false, // <
        replaceGreaterThanSign: false, // >
        replaceAmpersand: true, // &
        replaceDoubleQuote: true, // "
        replaceSingleQuote: false // '
    }
) {
    const entityMap = {
        '&lt;': '<',
        '&gt;': '>',
        '&amp;': '&',
        '&#39;': "'"
    };

    // 原本的Json字串中的特殊字元首先可能會被HtmlEncode編碼為 &... 
    // 其中的 & 在輸出至View時會再被編碼為&amp;
    // 所以要先將&amp;解碼回來，再看看要不要解碼為其他字元

    for (const [entity, char] of Object.entries(entityMap)) {
        if (options[`replace${char === '<' ? 'LessThanSign' :
            char === '>' ? 'GreaterThanSign' :
                char === '&' ? 'Ampersand' :
                    char === '"' ? 'DoubleQuote' :
                        'SingleQuote'}`]) {
            encodedJsonStr = encodedJsonStr.replaceAll(entity, char);
        }
    }

    // 因為Json字串中總是會有雙引號(")，所以雙引號總是需要解回來
    encodedJsonStr = encodedJsonStr.replaceAll('&quot;', '"');

    return JSON.parse(encodedJsonStr);
}

function isNullOrWhiteSpace(str) {
    return !str || str.trim().length === 0;
}

//For Checkmarx fix XSS
function sanitizeHTML(input) {
    // 用於編碼 HTML 特殊字符
    const element = document.createElement('div');
    if (input) {
        element.innerText = input;  // 設置為 innerText 而非 innerHTML
        element.textContent = input;  // 設置為 textContent
    }
    return element.innerHTML;  // 會自動將特殊字符轉換為 HTML 實體
}


TSC_NS.Configs = {
    masterTableComponent: {
        isAutoSelectLatestRow: true,         // 是否自動選擇最近異動的Row
        isAutoSelectTopRow: true,            // 當無異動Row時，是否自動選擇Top Row
    },
    fetchedPageModal: { // fetchedPageModal為彈出視窗
        liveTime: 1000, //視窗自動關閉時間, closeOnSuccess為true才有效
    },
};