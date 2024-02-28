import { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    children: [
      // Define 'accounts' as a child route
      {
        path: 'accounts', // Kein führender Schrägstrich, um 'accounts' als Kind von '/' zu definieren
        component: () => import('components/KaesseliAccounts.vue')
      },
      {
        path: 'transactions', // Kein führender Schrägstrich, um 'accounts' als Kind von '/' zu definieren
        component: () => import('components/KaesseliTransactions.vue')
      }
      // Hier können weitere Kinderrouten hinzugefügt werden
    ]
  },
  // Always leave this as last one,
  // but you can also remove it
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  }
];

export default routes;
