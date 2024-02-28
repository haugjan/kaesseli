<template>
  <div>
   xx
  </div>
</template>
<script lang="ts">
    import { defineComponent, watch} from 'vue';
    export default defineComponent({
      setup(props) {

        watch(() => props.accountId, (newVal) => {
          if (newVal) {
            /*FetchSummaries(newVal);*/
          }
        }, { immediate: true });

      },
      props: {
        accountId: {
          type: String,
          required: true,
        },
      },
    });
</script>
<style>
</style>
