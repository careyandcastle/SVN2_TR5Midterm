/// 因為底層自動產生的快速查詢有些問題
///   1. 無法限制只能輸入日期或數字
///   2. 無法使用選單
/// 每個用Vue.extend自己拉又很麻煩
///
/// 參考 https://github.com/yourtion/vue-json-ui-editor 改造來的
///
//////////////////////////////////////////////////
/// 屬性:
///   schema(必填):各欄位的語法
///   selectOptions:選單內容
///   valueObj(必填):被綁定的資料物件
///   resetButtonText:重置按鈕顯示的文字
/// 事件:
///   field-change(key, value):資料內容改變
///   reset-all():重置被點擊
//////////////////////////////////////////////////
/// 基本用法：
/// 1. 在index.aspx引用
/// ```
/// <script src = "~/js/ui/multi-input.js" asp-append-version="true" ></script>
/// ```
/// 2. 取代原有的filter-row:slot
/// ```
/// <template #filter-row="slot">
///     <b-row class="mb-2">
///         <b-col>
///             <multi-input :schema="slot.data.params.data.querySchema"
///                 :select-options="slot.data.params.data.querySelectOptions"
///                 :value-obj="slot.data.params.data.queryData"
///                 v-on:field-change="slot.data.queryChanged"
///                 v-on:reset-all="slot.data.queryResetAll">
///             </multi-input>
///         </b-col>
///     </b-row>
/// </template >
/// ```
/// 3. 定義可查詢的資料語法
///   key:鍵值
///   label:顯示名稱, 未定義時以key帶入
///   type:輸入類型, b-form-input支援的類型再加上select
///   mask:輸入遮罩, 請參考maska的官方文件, 當type為text才會生效
///   tokens:自訂遮罩token, 請參考maska的官方文件, 當type為text才會生效
///   reversed:是否反向遮罩, 當type為text才會生效
/// ```
/// const _querySchema = Vue.reactive([
///     {
///         key: 'prop1',
///         type: 'text',
///     },
///     {
///         key: 'prop2',
///         label: 'prop2_name',
///         type: 'select',
///     },
///     {
///         key: 'prop3',
///         label: 'prop3_name',
///         type: 'date',
///     },
///     {
///         key: 'prop4',
///         label: 'ANY_LENGTH_INTEGER',
///         type: 'text',
///         mask: '9#',
///         tokens: '9:[0-9]:repeated|0:[0-9]:optional',
///         reversed: true,
///     },
///     {
///         key: 'prop5',
///         label: 'DECIMAL(6,2)',
///         type: 'text',
///         mask: '#000.00',
///         tokens: '9:[0-9]:repeated|0:[0-9]:optional',
///     },
/// ]);
/// ```
/// 4. 如果有用到選單, 定義選項
/// const _querySelectOptions = Vue.reactive({
///     prop2: [
///         { value: '', text: '請選擇' },
///         { value: '選項一', text: '選項一' },
///     ]
/// });
/// 5. 定義資料變數, 如果有初始值請一併先給定
/// ```
/// const _queryData = Vue.reactive({
///   prop1 : 'TEST1_DEFAULT_VALUE'
/// });
///
/// ```
/// 6. 處理資料變更, 如果使用連動選單, 請在此事件處理
/// ```
/// const _queryChanged = function (key, value) {
///    let filterDataObj = {};
///    this.params.querySchema.forEach((item) => {
///        let val = this.params.queryData[item.key];
///        if (val != null && val != '') {
///            filterDataObj[item.key] = { condit: 'eq', value: val };
///        }
///    });
///    _assignFilter.call(this, filterDataObj);
/// }
/// ```
///
/// 7. 處理資料重置, 如果使用連動選單, 請自行還原querySelectOptions內容
/// ```
/// const _queryResetAll = function () {
//     _assignFilter.call(this, null);
/// }
/// ```
/// 8. 記得注入這些變數和方法
/// ```
/// params.data = {
///     ...
///     querySchema: _querySchema,
///     querySelectOptions: _querySelectOptions,
///     queryData: _queryData,
/// }
/// params.methods = {
///     ...
///     queryChanged: _queryChanged,
///     queryResetAll: _queryResetAll,
/// };
/// ```
/// 9. 如果schema和options不會變動也可以直接寫在component內
/// ```
/// <multi-input 
///     :schema="[{key:'進銷存組織1', label:'進銷存組織', type:'select'},{key:'日期', type:'date'}]"
///     :select-options="{進銷存組織1:[{value:'1', text:'選項一'},{value:'2', text:'選項二'}]}"
///     :value-obj="slot.data.params.data.queryData"
///     v-on:field-change="slot.data.queryChanged"
///     v-on:reset-all="slot.data.queryResetAll">
/// </multi-input>
/// ```
/// 10. 如果有些component要自己寫請用slot取代
/// slot名稱為 input_{item.key}, 例 input_日期
/// slot提供以下prop供使用
///   item : 本列的schema
///   currentValue: 對應值, 即props.value[item.key]
///   currentSelectOptions : 對應選單, 即selectOptions[item.key]
///   changed(key, value): 供input送出change event使用, @change=changed(item.key, $event)
///   updateValue(key, value): 供input更新資料使用, @input=updateValue(item.key, $event)
/// ** changed及updateValue均會更新查詢資料, 但只有changed會進一步觸發field-change事件
/// ** 如果所使用的輸入元件只有一種輸入變動事件, 那呼叫changed即可
/// ```
/// <template #input_日期="{item, currentValue, currentSelectOptions, changed, updateValue}">
///     <b-form-datepicker id="example-datepicker"
///         :value="currentValue"
///         @input="changed(item.key, $event)"
///         class="mb-2">
///     </b-form-datepicker>
/// </template >
/// ```
/// 11. 連動選單例子
/// ```
/// const _queryChanged = function (key, value) {
///     if (key == '進銷存組織') {
///         let options = ConvertSelectListItemsToBFormSelectOptions(CallApi取得倉庫代號選單Sync(value));
///         _querySelectOptions["倉庫代號"] = options;
///         if (options.length > 0) {
///             queryData["倉庫代號"] = options[0].value;
///         }
///         else {
///             queryData["倉庫代號"] = ""; /// 清空選取值
///         }
///     }
///     ...
/// ```



(function () {
    const templateHtml = `
<b-row>

    <b-col 
        v-for="(item, key) in schema"
        :key="item.key + '_' + key"
        :sm="(item.grid_options && item.grid_options.sm) || 12"
        :md="(item.grid_options && item.grid_options.md) || 6"
        :lg="(item.grid_options && item.grid_options.lg) || 4"
        :xl="(item.grid_options && item.grid_options.xl) || 3"
        :class="item.class ?? 'mt-2'"
        :style="item.style"
        v-show="showinputfilter">
        <slot
            :name="'input_' +  item.key"
            :item=item
            :current-value=valueObj[item.key]
            :current-select-options=selectOptions[item.key]
            :changed=changed
            :update-value=updateValue
            >
            <general-input-editor
                :label="item.label ?? item.key"
                :type="item.type"
                v-model="valueObj[item.key]"
                :select-options="selectOptions[item.key]"
                :mask=item.mask
                :tokens=item.tokens
                :reversed=item.reversed
                :is-multiple=item.isMultiple
                :is-disabled=item.isDisabled
                :input_id="item.id ?? 'input_' +  item.key"
                :always-show-label=item.alwaysShowLabel
                :use-boostrap-buttons=item.useBoostrapButtons
                @change="changed(item.key, $event)"
            ></general-input-editor>
        </slot>
    </b-col>
    <b-col :class="justifyClass"><!--調整按鈕位置-->
        <slot name="front-button-slot"></slot>
        <slot name="mid-button-slot">
        <b-button
                @click="filterClicked"
                style="max-height:38px; white-space:nowrap"
                class="mt-2 mr-2"
                variant="detail3"
                :disabled="!isAnyDataInput"
                v-show="showquerybtn"
            >{{filterButtonText}}</b-button> <!--新增查詢按鈕-->
        <b-button
            @click="resetClicked"
            style="max-height:38px; white-space:nowrap"
            class="mt-2 mr-2"s
            variant="detail3"
            v-show="showcleanbtn"
            :disabled="!isAnyDataInput"
        >{{resetButtonText}}</b-button>
        <b-button
            @click="exportClicked"
            style="max-height:38px; white-space:nowrap"
            class="mt-2 mr-2"s
            variant="detail3"
            v-show="showexportbtn"
            :disabled="!isAnyDataInput"
        >{{exportButtonText}}</b-button>
  </slot>
        <slot name="tail-button-slot"></slot>
    </b-col>

</b-row>
 `;

    const componentObject = {
        template: templateHtml,
        props: {
            /**
             * 各欄位的語法, 要有
             *  1. key:對應的查詢欄位, 要與data內的資料一致
             *  2. label(optional):名稱, 未輸入時自動帶入key
             *  3. type:欄位類型, 接受text, password, email, number, url, tel, search,
             *     date, datetime, datetime-local, month, week, time, range, color or select
             *     預設是text
             *  4. mask(optional):遮罩內容, 請參考maska的官方文件, 當type為text才會生效
             *  5. tokens(optional):自訂遮罩token, 請參考maska的官方文件, 當type為text才會生效
             *  6. reversed(optional):是否反向遮罩, 請輸入boolean, 當type為text才會生效
             * [
             *  {
             *   key:'FIELD1_KEY',
             *   label:'FIELD1_NAME',
             *   type:'FIELD1_TYPE',
             *   mask:'MASK_FORMAT'
             *   tokens:'MASK_TOKENS',
             *   reversed: true|false
             *  },
             *  ...
             * ]
             */
            schema: { type: Array, required: true },

            /**
             * 選單內容
             * {
             *    FIELD_KEY1:
             *    [
             *      {value:OPTION1_VALUE, text:OPTION1_TEXT}
             *      , ....
             *    ]             
             * }
             * @model
             * @default {}
             */
            selectOptions: { type: Object, default: () => ({}) },

            /**
             * 被綁定的資料物件, 如果有預設值, 請一開始一併設置, 重置時會回復設定
             * 頁面資料輸入又清除後會變為空值而不是null
             * @model
             * @default {}
             */
            valueObj: { type: Object, required: true, default: () => ({}) },
            /**
             * 查詢按鈕顯示的文字
             * @model
             * @default "查詢"
             */
            filterButtonText: { type: String, default: "查詢" },
            /**
             * 重置按鈕顯示的文字
             * @model
             * @default "清除過濾"
             */
            resetButtonText: { type: String, default: "清除" },
            /**
             * 匯出按鈕顯示的文字
             * @model
             * @default "匯出"
             */
            exportButtonText: { type: String, default: "匯出" },

            showexportbtn: { type: Boolean, default: true },
            showquerybtn: { type: Boolean, default: true },
            showcleanbtn: { type: Boolean, default: true },
            showinputfilter: { type: Boolean, default: true },

            // 用於控制對齊方向
            justifyContent: { type: String, default: "end" } // 預設為 "end"，可傳入 "start" 等值

        },

        /// event
        /// field-change : 欄位改變事件, 如果有連動選單, 請在此處理
        ///     param1: 被改變的欄位key值
        ///     param2: 改變後的值
        /// reset-all : 重置按鈕觸發之事件, 如果有連動選單, 請在此自行重置

        setup(props, context) {
            /// computed propery 只會監控一開始有的物件屬性            
            props.schema.forEach((item) => {
                Vue.set(props.valueObj, item.key, props.valueObj[item.key] ?? '');
            });

            /// 備份一開始的狀態
            const defaultData = {
            };

            props.schema.forEach((item) => {
                defaultData[item.key] = props.valueObj[item.key] ?? '';
            });

            const changed = (key, value) => {
                props.valueObj[key] = value;
                context.emit('field-change', key, value);
            };

            /// 雖然data是物件, 所以slot可以變更值
            /// 但官方建議透過callback去改
            /// 供slot呼叫去變更值
            const updateValue = (key, value) => {
                props.valueObj[key] = value;
            };

            const resetClicked = () => {
                props.schema.forEach((item) => {
                    props.valueObj[item.key] = defaultData[item.key];
                });
                context.emit('reset-all');
            };

            const isAnyDataInput = Vue.computed(
                () => {
                    return props.schema.some((item) => {
                        return props.valueObj.hasOwnProperty(item.key) && props.valueObj[item.key] != null && props.valueObj[item.key] != '';
                    });
                }
            );

            //當按下查詢按鈕
            const filterClicked = () => {
                context.emit("filter-click");
            };

            //當按下匯出按鈕
            const exportClicked = () => {
                context.emit("export-click");
            };

            const justifyClass = Vue.computed(() => {
                return `d-flex justify-content-${props.justifyContent}`;
            });

            //記得要return自己寫的clicked function物件如(filterClicked)，不然按鈕會沒有作用
            return {
                changed, resetClicked, isAnyDataInput, updateValue,
                filterClicked, exportClicked, justifyClass
            }
        },
    };

    const componentName = 'multi-input'

    Vue.component(
        componentName,
        componentObject
    );

})();