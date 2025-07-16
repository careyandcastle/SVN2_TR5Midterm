Vue.component('button-counter', {
	data: function () {
		return {
			count: 0,
		};
	},
	template: `
    <button @click="count++">
      You clicked me {{ count }} times.
    </button>
  `,
});
Vue.component('org-clock-info', {
    props: ['事業', '單位', '部門', '分部'],
    data() {
        return {
            now: new Date().toLocaleTimeString()
        };
    },
    mounted() {
        this.timer = setInterval(() => {
            this.now = new Date().toLocaleTimeString();
        }, 2000);
    },
    beforeDestroy() {
        clearInterval(this.timer);
    },
  //  template: `
  //  <div class="card shadow-sm p-3" style="max-width: 360px;">
  //    <div class="mb-2 text-muted">
  //      <i class="bi bi-clock"></i> 現在時間：
  //      <strong class="text-primary">{{ now }}</strong>
  //    </div>
  //    <div><strong>事業：</strong>{{ 事業 }}</div>
  //    <div><strong>單位：</strong>{{ 單位 }}</div>
  //    <div><strong>部門：</strong>{{ 部門 }}</div>
  //    <div><strong>分部：</strong>{{ 分部 }}</div>
  //  </div>
  //`
    template: `<div class="card shadow-sm p-3">
    <div class="row align-items-center flex-wrap">
      <!-- 🕒 現在時間 -->
      <div class="col-auto text-muted d-flex align-items-center me-4 mb-2">
        <i class="bi bi-clock me-1"></i>
        <span>現在時間：</span>
        <strong class="text-primary ms-1">{{ now }}</strong>
      </div>

      <!-- 📌 組織資訊 -->
      <div class="col-auto text-muted me-4 mb-2">
        <strong>事業：</strong> {{ 事業 }}
      </div>
      <div class="col-auto text-muted me-4 mb-2">
        <strong>單位：</strong> {{ 單位 }}
      </div>
      <div class="col-auto text-muted me-4 mb-2">
        <strong>部門：</strong> {{ 部門 }}
      </div>
      <div class="col-auto text-muted me-4 mb-2">
        <strong>分部：</strong> {{ 分部 }}
      </div>
    </div>
  </div>`
});