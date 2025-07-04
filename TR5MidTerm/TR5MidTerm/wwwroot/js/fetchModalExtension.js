const MultiSelector = {
    template: `            
<div style="position: relative;" class="tscMultiSelect">
    <vue-multi-select v-model="selected"
                        label="text" track-by="value" :placeholder="placeholder"
                        :preserveSearch="true" :show-labels="false" :prevent-autofocus="false"
                        :options="dataSource"                        
                        :name="name+'-multi-sel'" :disabled="disabled"
                        :loading="isOnLoading"
                        :max-height=240
                        ref="multiSelect"
                        @input="onChange" @open="onOpen" @close="onClose">

        <template #noResult>
            <span>查無資料</span>
        </template>

        <template #noOptions>
            <span>查無資料</span>
        </template>
    </vue-multi-select>
    <input ref="selectedInput" type="hidden" :name="name" :value="selected?.value"/>
</div>`,
    props: {
        name: {},
        defaultValue: {},
        placeholder: { default: '-請選擇-' },
        searchable: { default: false },
        changeHandler: {
            default: () => { }
        },
        disabled: {
            default: false,
        },
    },
    data() {
        return {
            isOnLoading: false,
            selected: null,
            dataSource: [],
        }
    },
    methods: {
        async onChange(item) {
            this.changeHandler(item, this);
        },
        onOpen() {
            let $ele = $(this.$el);
            $ele.closest(".b-table-sticky-header").one('wheel', (function (event) {
                event.stopPropagation();
                this.$refs.multiSelect.deactivate();
            }).bind(this));

            const $listBox = $ele.find('div.multiselect__content-wrapper')
                .css({
                    position: '',
                    top: this.$el.getBoundingClientRect().bottom - 2,
                    width: $ele.width() - 1,
                })
                .on('wheel', event => event.stopPropagation());

            const spaceBelow = window.innerHeight
                - this.$el.getBoundingClientRect().bottom;

            const minHeight = Math.min((this.dataSource.length + 1) * 40, this.$refs.multiSelect.maxHeight);
            const canShowOnBottom = spaceBelow >= minHeight - 2;
            this.$refs.multiSelect.openDirection = '';

            if (canShowOnBottom) {
                this.$refs.multiSelect.openDirection = 'below';
                return;
            }

            const tabsEle = $ele.closest('.tabs').get(0);
            const spaceAbove = this.$el.getBoundingClientRect().top
                - tabsEle.getBoundingClientRect().top;
            const canShowOnTop = spaceAbove > minHeight + 5;

            if (canShowOnTop) {
                this.$refs.multiSelect.openDirection = 'above';
                this.$refs.multiSelect.optimizedHeight = minHeight;
                $listBox.css({
                    top: 'auto',
                    position: 'absolute',
                });
                return;
            }
        },
        onClose() {
            $(this.$el).find('.multiselect__content-wrapper').off('wheel');
        },
        setOptions(dataSource) {
            this.dataSource = dataSource ?? [];
            this.selected = this.dataSource.find(item => item.value == this.defaultValue?.value) ?? null;
            this.onChange(this.selected ?? { value: null });

        },
        unSelect() {
            this.selected = null;
        }
    },
    mounted() {
        if (this.defaultValue) {
            this.selected = this.defaultValue;
        }
    },
};
Vue.component('multi-selector', MultiSelector);

const GetElementGroupName = function (element) {
    //name: '租約明細檔[N].單價'
    element = (typeof element === 'object') ? element : { name: element };
    return element.name.split('.')[0]; //['租約明細檔[N]', '單價'] -> '租約明細檔[N]'
}

const setTabKeyNavigationDirect = function (table, isVertical = false) {
    const $currTable = $(table);
    //左右
    if (!isVertical) {
        $currTable.find('input, select, textarea').attr('tabindex', 0);
        return;
    }

    //上下   
    const $currTableTR = $currTable.find('tr');
    const rowCount = $currTableTR.length;

    $currTableTR.each(rowIdx => {
        const $tdInOneRow = $currTableTR.eq(rowIdx).find('td');

        $tdInOneRow.each(colIdx => {
            const $editableEles = $tdInOneRow.eq(colIdx).find('input, select, textarea')
                .not('[type=hidden], [disabled], [readonly], [contenteditable=false]').first();

            $editableEles.attr('tabindex', rowIdx + colIdx * rowCount);
        });
    });
}