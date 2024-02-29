import { RouteRecordRaw } from 'vue-router';

// ReSharper disable TsResolvedFromInaccessibleModule
const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [
      {
        path: 'accounts', 
        component: () => import('components/KaesseliAccounts.vue')
      },
      {
        path: 'accountTable/:id',
        component: () => import('components/KaesseliAccountTable.vue')
      },
      {
        path: 'transactions', 
        component: () => import('components/KaesseliTransactions.vue')
      }
      ,
      {
        path: 'import',
        component: () => import('components/KaesseliImport.vue')
      },
      {
        path: 'assign',
        component: () => import('components/KaesseliAssign.vue')
      }
      
    ]
  },
  
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  }
];

export default routes;
// ReSharper restore TsResolvedFromInaccessibleModule
