# ScrapeBootstrapVue

I'm helping out with a project called [bootstrap-vue-next](https://github.com/bootstrap-vue-next/bootstrap-vue-next)
that is a re-implementation of [bootstrap-vue](https://github.com/bootstrap-vue/bootstrap-vue) using 
[Vue3](https://vuejs.org/) and [Bootstrap5](https://getbootstrap.com/) (bootstrap-vue is based on 
(Vue2)[https://v2.vuejs.org/] and (Bootstrap4)[https://getbootstrap.com/docs/4.6]).

One thing we want to do is create a parity/completeness report as a spreadsheet and seed it with the component reference
information from bootstrap-vue.  I looked at getting the information directly from the code in bootstrap-vue, but it's
in a couple of different places and there is a bunch of code used to weave it together, so I opted to just scrape the
documentation website for the information I needed, since it's already woven together in the way I want.
