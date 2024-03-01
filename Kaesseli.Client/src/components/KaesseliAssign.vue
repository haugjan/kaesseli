<template>
  <div v-if="transaction" class="row q-pa-md">
    <q-card flat bordered class="my-card">
      <q-card-section class="col-md-6 col-sm-12">
        <div class="text-h6">
          {{formatDate( transaction.valueDate)}}
          <p>
            <q-chip v-if="transaction.amount >= 0" color="green-9" text-color="white" icon="attach_money">
              {{formatNumber(transaction.amount)}}
            </q-chip>
            <q-chip v-if="transaction.amount < 0" color="red-14" text-color="white" icon="money_off">
              {{ formatNumber(transaction.amount)}}
            </q-chip>
          </p>
        </div>
      </q-card-section>

      <q-card-section class="q-pt-none">
        {{transaction.accountName}}
      </q-card-section>

      <q-separator inset />

      <q-card-section class="q-pt-none">
        {{transaction.description}}
      </q-card-section>
    </q-card>
  </div>
  <div v-if="transaction" class="row q-pa-md">
    <div v-for="account in transaction.suggestedAccounts" :key="account.id">
      <q-chip @click="onClick(account)" size="md" :color="account.accountIconColor" :icon="account.accountIcon" text-color="white" square> {{account.accountName}}</q-chip>
      
    </div>
  </div>
</template>

<script lang="ts">

  interface ISuggestedAccount {
    accountName: string;
    accountType: string;
    accountTypeId: number;
    accountIcon: string;
    accountIconColor: string;
  }

  interface ITransaction {
    transactionId: string;
    amount: number;
    description: string;
    valueDate: Date;
    accountName: string;
    accountType: string;
    accountTypeId: number;
    suggestedAccounts: ISuggestedAccount[];
  }

  import { defineComponent, ref, onMounted } from 'vue';
  import axios from 'axios';

  export default defineComponent({
    setup() {

      const transaction = ref<ITransaction | null>(null);

      const FetchTransaction = async () => {
        try {
          const response = await axios.get(`https://localhost:7123/transaction/nextOpen`);
          transaction.value = response.data;
        } catch (error) {
          console.error('There was an error fetching the transactions:', error);
        }
      };


      const formatDate = (dateStr: Date) => {
        const date = new Date(dateStr);
        return new Intl.DateTimeFormat('de-CH', { // 'de-CH' für Schweizerdeutsch, um das gewünschte Format zu erhalten
          weekday: 'long', // volle Bezeichnung des Wochentags
          year: 'numeric',
          month: 'long', // volle Bezeichnung des Monats
          day: '2-digit'
        }).format(date);
      };

      function formatNumber(value: number, locale: string = navigator.language): string {
        return new Intl.NumberFormat(locale, {
          style: 'decimal',
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        }).format(value);
      }

      function onClick(transaction: ITransaction) {

      }

      onMounted(() => {
        FetchTransaction();
      });

      return {
        FetchTransaction,
        transaction,
        formatDate,
        formatNumber,
        onClick
      }

    }
  });
</script>

<style>
</style>
