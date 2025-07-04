//////////////////////////////////////////////////
/// 供搜尋產生風格一致之輸入欄位使用, 如果有特殊需求請自己拉
/// 屬性:
///  1. labeL:名稱
///  2. type:欄位類型, 接受text, password, email, number, url, tel, search,
///     date, datetime, datetime-local, month, week, time, range, color or select
///     預設是text
///  3. select-options:select的選單([{value:'', text, ''}, ...])
///  4. mask(optional):遮罩內容, 請參考maska的官方文件, 當type為text才會生效
///  5. tokens(optional):自訂遮罩token, 請參考maska的官方文件, 當type為text才會生效
///  6. reversed(optional):是否反向遮罩, 請輸入boolean, 當type為text才會生效
///
/// 事件:
///  1. input($event) : input事件, vue會自己處理
///  2. change($event) : input的change事件
///
/// 基本用法：
/// 1. 引用
/// ```
/// <script src = "~/js/ui/general-input-editor.js" asp-append-version="true" ></script>
/// ```
/// 2. 與值綁定即可
/// ```
/// <general-input-editor label="進銷存組織" type="text"></general-input-editor>
/// <general-input-editor label="日期" type="date"></general-input-editor>
/// <general-input-editor label="流水號" type="text" mask="#000.00" tokens="9:[0-9]:repeated|0:[0-9]:optional"></general-input-editor>
/// <general-input-editor label="下拉選單" type="select" :select-options= "[{value: '進銷存組織1', text: '進銷存組織代號'}]"></general-input-editor>
/// ```
/// 3. 可輸入欄位
/// ```
///     label:欄位名稱
///     type:輸入類型(未輸入以text帶入)
///     v-model:輸入值
///     select-options=select的選單([{value:'', text, ''}, ...])
///     mask:輸入遮罩(選填), 請參考maska的官方文件, 當type為text才會生效
///     tokens:自訂遮罩token(選填), 請參考maska的官方文件, 當type為text才會生效
///     reversed:true|false是否反向遮罩(選填), 當type為text才會生效
///     所有input的事件
///     
///     PE2增加:
///     isMultiple:讓select變為多選
///     input_id:指定input或select的id
///     isDisabled:將input或select給disable
///     alwaysShowLabel:可讓input或select即使有輸入值，也不會主動隱藏label(標題)
/// ```
///////////////////////////////////////

(function () {
    const templateHtml = `
<div>
    <template v-if="type == 'select'">
        <!--因為select, 不會顯示placeholder, 一般是用第一個選項處理, 所以用inputgroup去顯示標題-->
        <b-input-group :prepend="(value && value.length > 0 && value !== '%') ? (alwaysShowLabel == true ? (label ?? key) : '' ) : (label ?? key)">
            <b-form-select
                :multiple=isMultiple
                :options="selectOptions"
                v-b-tooltip.bottom.v-secondary
                :title="label ?? key"
                :value="value"
                :id="input_id"
                :disabled=isDisabled
                v-on="{
                    ...$listeners,
                    input: event => $emit('input', event)
                }"
            >
            </b-form-select>
        </b-input-group>
    </template>
    <template v-else-if="type == 'date'">
        <!--因為type='date', 不會顯示placeholder, 動態改變type會造成資料被清空, 所以用inputGroup顯示-->
        <b-input-group :prepend="(value ?? '' != '') ? (alwaysShowLabel == true ? (label ?? key) : '' ) : (label ?? key)">
            <b-form-input
                :placeholder="label ?? key"
                :type="type ?? 'text'"
                v-b-tooltip.bottom.v-secondary
                :title="label ?? key"
                :value="value"
                :id="input_id"
                :disabled=isDisabled
                @blur="validateDate"
                v-on="{
                    ...$listeners,
                    input: event => $emit('input', event)
                }"
            >
            </b-form-input>
        </b-input-group>
    </template>
    <template v-else-if="type == 'radio'">
        <b-input-group :prepend="(alwaysShowLabel == true ? (label ?? key) : '' )" class="flex-nowrap">
                <b-form-radio-group 
                    :type="type ?? 'text'"
                    :title="label ?? key"
                    :options="selectOptions"
                    :disabled=isDisabled
                    v-model="value"
                    :buttons="useBoostrapButtons"
                    :button-variant="useBoostrapButtons ? 'outline-secondary' : ''"
                    v-on="{
                        ...$listeners,
                        input: event => $emit('input', event)
                    }">
                </b-form-radio-group>
        </b-input-group>
    </template>
    <template v-else>
        <b-form-input
            :placeholder="label ?? key"
            :type="type ?? 'text'"
            v-b-tooltip.bottom.v-secondary
            :title="label ?? key"
            :value="value"
            :id="input_id"
            :disabled=isDisabled
            v-on="{
                ...$listeners,
                input: event => $emit('input', event)
            }"
            v-maska
            :data-maska="((type ?? 'text') == 'text') ? mask : null"
            :data-maska-tokens="((type ?? 'text') == 'text') ? tokens : null"
            :data-maska-reversed="(((type ?? 'text') == 'text') && (reversed == true || reversed == 'true')) ? true : null"
        >
        </b-form-input>
    </template>
</div>
 `;

    const componentObject = {
        template: templateHtml,
        props: {
            /**
             *  要有
             *  1. label:欄位名稱
             *  2. type:欄位類型, 接受text, password, email, number, url, tel, search,
             *     date, datetime, datetime-local, month, week, time, range, color or select
             *     預設是text
             *  3. mask(optional):遮罩內容, 請參考maska的官方文件, 當type為text才會生效
             *  4. tokens(optional):自訂遮罩token, 請參考maska的官方文件, 當type為text才會生效
             *  5. reversed(optional):是否反向遮罩, 請輸入boolean, 當type為text才會生效
             */
            /// label(optional):名稱
            label: { type: String, required: true },

            /// type:欄位類型, 接受text, password, email, number, url, tel, search,
            ///   date, datetime, datetime- local, month, week, time, range, color or select
            ///   預設是text
            type: { type: String, default:"text" },

            /// mask(optional):遮罩內容, 請參考maska的官方文件, 當type為text才會生效
            mask: { type: String },

            /// tokens(optional):自訂遮罩token, 請參考maska的官方文件, 當type為text才會生效
            tokens: { type: String },

            /// 是否反向遮罩, 請輸入boolean, 當type為text才會生效
            reversed: { type: Boolean, default: false },
            /**
             * 選單內容
             * [
             *   {data:OPTION1_DATA, text:OPTION1_TEXT}
             *    , ....
             * ]             
             * @model
             * @default {}
             */
            selectOptions: { type: Array, default: () => ([]) },

            /**
             * 被綁定的資料
             * 頁面資料輸入又清除後會變為空值而不是null
             * @model
             * @default {}
             */
            value: { type: [String, Number] },

            /**
            * input的id
            * 預設是'input_' +  item.key
            * @model
            * @default {}
            */
            input_id: { type: String },

            /**
             * 讓select選單變為可多選
             * 對其他的input不會有作用
             * @model
             * @default {}
             */
            isMultiple: { type: Boolean, default: false },

            /**
             * 是否disable欄位
             * @model
             * @default {}
             */
            isDisabled: { type: Boolean, default: false },

            /**
             * multi-input預設會在使用者輸入(選擇)後隱藏label
             * 這邊讓它可以選擇要永遠顯示
             * @model
             * @default {}
             */
            alwaysShowLabel: { type: Boolean, default: false },

            /**
            * 每個radio button顯示的文字與其value
            * @model
            * @default {}
            */
            radioButtons: { type: Array, default: () => ([{ text: '是', value: true }, { text: '否', value: false }]) },

            /**
            * 是否要讓radio button使用boostrap-vue的buttons屬性
            * @model
            * @default {}
            */
            useBoostrapButtons: { type: Boolean, default: false },

            /**
            * dateTimePicker預設的date之上下限
            * @model
            * @default {}
            */
            minDate: { type: Date, default: () => { return new Date('1900-01-01') } },
            maxDate: { type: Date, default: () => { return new Date('9999-12-31') }},
        },

        /// event : 所有input的事件        

        setup(props, context) {

            //限制date的輸入範圍，避免送到後端並轉為dateTime時發生爆炸
            const validateDate = () => {
                if (props.type === 'date') {
                    const dateValue = new Date(props.value);
                    if (props.minDate && dateValue < props.minDate) {
                        context.emit('input', FormatDate(props.minDate));
                    } else if (props.maxDate && dateValue > props.maxDate) {
                        context.emit('input', FormatDate(props.maxDate));
                    }
                }
            };

            return { validateDate }
        },
    };

    function FormatDate(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    const componentName = 'general-input-editor'

    Vue.component(
        componentName,
        componentObject
    );

})();